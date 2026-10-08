# MoneyFlow

После сохранения счёта API публикует событие `AccountCreated` в RabbitMQ.
Отдельное приложение `MoneyFlow.Notifications.Worker` получает событие
и записывает уведомление в лог.

## Требования

- .NET SDK 10.
- Docker с поддержкой Docker Compose.
- PowerShell для выполнения примеров.
- dotnet-ef версии 10.0.9 для применения миграций.

Если dotnet-ef ещё не установлен:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.9
```

## Настройка

Из корня репозитория перейдите в папку решения:

```powershell
cd .\MoneyFlow
```

Если локальных файлов конфигурации ещё нет, создайте их по образцам:

- `.env.example` → `.env`;
- `MoneyFlow.Api/appsettings.example.json` → `MoneyFlow.Api/appsettings.json`;
- `MoneyFlow.Notifications.Worker/appsettings.example.json` → `MoneyFlow.Notifications.Worker/appsettings.json`.

В `.env` задайте параметры PostgreSQL и RabbitMQ.

В конфигурации API заполните `ConnectionStrings:Default`.
Имя базы, порт, пользователь и пароль должны соответствовать значениям
`POSTGRES_DB`, `POSTGRES_PORT`, `POSTGRES_USER` и `POSTGRES_PASSWORD`.

В секции `RabbitMq` у API и Worker задайте одинаковые настройки:

| Параметр | Значение для локального запуска |
| --- | --- |
| HostName | localhost |
| Port | Значение RABBITMQ_AMQP_PORT, по умолчанию 5672 |
| VirtualHost | / |
| UserName | Значение RABBITMQ_DEFAULT_USER |
| Password | Значение RABBITMQ_DEFAULT_PASS |
| QueueName | account-created |

Файл `.env` используется Docker Compose. API и Worker автоматически
его не читают, поэтому настройки подключения нужно заполнить отдельно.

Локальные файлы с паролями исключены из Git.

## Запуск

Все команды ниже выполняются из папки `MoneyFlow`.

Запустите PostgreSQL и RabbitMQ:

```powershell
docker compose up -d postgres rabbitmq
docker compose ps
```

Дождитесь готовности PostgreSQL и открытия RabbitMQ Management.

Примените миграции:

```powershell
Push-Location .\MoneyFlow.Infrastructure
dotnet ef database update --startup-project ..\MoneyFlow.Api
Pop-Location
```

Переход в папку Infrastructure нужен текущей фабрике DbContext
для поиска конфигурации API.

В первом терминале запустите API:

```powershell
dotnet run --project .\MoneyFlow.Api --launch-profile http
```

Во втором терминале запустите Worker:

```powershell
dotnet run --project .\MoneyFlow.Notifications.Worker
```

Адреса при стандартных настройках:

- API: http://localhost:5026
- Проверка работы API: http://localhost:5026/health/live
- Проверка подключения к PostgreSQL: http://localhost:5026/health/ready
- RabbitMQ Management: http://localhost:15672

Для входа в RabbitMQ Management используйте логин и пароль из `.env`.

Очередь `account-created` создаётся при запуске Worker
или при первой публикации события API.

## Создание счёта

Для запроса нужен существующий пользователь в таблице `users`.
Миграции не создают пользователей.

Для пустой локальной базы можно добавить демонстрационного пользователя.
Откройте psql:

```powershell
docker compose exec postgres sh -c 'psql -U "$POSTGRES_USER" -d "$POSTGRES_DB"'
```

Выполните SQL:

```sql
INSERT INTO users ("Id", "Email", "PasswordHash", "CreatedAt")
VALUES (
    gen_random_uuid(),
    'rabbitmq-demo@example.invalid',
    'demo-only-not-a-real-password-hash',
    CURRENT_TIMESTAMP
)
ON CONFLICT ("Email") DO NOTHING;

SELECT "Id"
FROM users
WHERE "Email" = 'rabbitmq-demo@example.invalid';
```

Значение PasswordHash здесь — заглушка для локальной проверки создания
счёта. Скопируйте полученный Id и выйдите из psql командой `\q`.

В отдельном терминале PowerShell выполните:

```powershell
$userId = Read-Host "Введите GUID существующего пользователя"

$body = @{
    userId = $userId.Trim()
    name = "RabbitMQ test $([Guid]::NewGuid().ToString('N'))"
    type = "Card"
    currencyCode = "RUB"
    openingBalance = 1000
} | ConvertTo-Json

$response = Invoke-WebRequest `
    -Method Post `
    -Uri "http://localhost:5026/api/accounts" `
    -ContentType "application/json" `
    -Body $body `
    -ErrorAction Stop

$response.StatusCode
$response.Content | ConvertFrom-Json
```

Ожидаемый результат:

- API возвращает `201 Created`.
- Счёт сохраняется в PostgreSQL.
- Worker пишет `Account created notification`.
- AccountId в уведомлении совпадает с Id созданного счёта.

Перед каждым повторным запросом заново сформируйте `$body`,
чтобы получить новое имя счёта.

## Событие AccountCreated

Событие передаётся в JSON и содержит:

| Поле | Назначение |
| --- | --- |
| EventId | Идентификатор события |
| AccountId | Идентификатор созданного счёта |
| UserId | Идентификатор владельца |
| CreatedAt | Время создания счёта |

При повторной доставке одного сообщения его EventId сохраняется.

## Проверка RabbitMQ

- Создать счёт: API возвращает 201, Worker пишет уведомление.
- Остановить Worker и создать счёт: сообщение остаётся в Ready.
  После запуска Worker очередь опустошается.
- Завершить Worker перед BasicAckAsync: после перезапуска приходит
  тот же EventId с Redelivered = True.
- Отправить not-json: Worker пишет предупреждение, удаляет сообщение
  без повторной доставки и продолжает работу.

## Ограничения

Publisher confirm подтверждает публикацию брокером.
Consumer ack подтверждает обработку сообщения Worker.

Повторная обработка возможна; дедупликация не реализована.
Некорректные сообщения удаляются, DLQ не настроена.

Сохранение счёта и публикация события не атомарны:
при ошибке публикации счёт может остаться в базе без события.
Transactional Outbox пока не реализован.