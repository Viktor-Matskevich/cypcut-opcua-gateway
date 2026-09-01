# CypCut → OPC UA Gateway

[English README](README.md)

Независимый Windows-шлюз для подключения нескольких станков CypCut к системам
MDC, MES, SCADA и промышленной аналитики через OPC UA.

> **Статус: Experimental — ожидается проверка на реальном станке.**
>
> Сборка, преобразование тестового JSON и одновременный запуск 10 OPC UA endpoint
> уже проверены. Маршрут получения данных должен быть подтверждён на конкретной
> установленной версии CypCut.

## Задача

Каждый включённый станок опрашивается по отдельному IP и порту. На центральном
Windows-сервере шлюз публикует для него отдельный OPC UA endpoint:

```text
192.0.2.101:8080 → opc.tcp://192.0.2.10:4880/CypCut/laser-01
192.0.2.102:8080 → opc.tcp://192.0.2.10:4881/CypCut/laser-02
```

Адреса `192.0.2.x` используются только как безопасные примеры. Их необходимо
заменить адресами своей производственной сети.

## Возможности

- один центральный Windows-сервис для нескольких станков;
- `Enabled=true/false` для каждого станка;
- настраиваемые IP, входной порт, OPC UA-порт и период опроса;
- отдельный OPC UA endpoint для каждого станка;
- 78 технологических и 9 диагностических переменных;
- сохранение полного последнего JSON в `Connection/RawJson`;
- отображение качества данных через OPC UA status codes;
- автоматический запуск как служба Windows.

## Конфигурация

Центральный сервер задаётся в `config/gateway.json`:

```json
{
  "name": "CypCut-Standalone-Gateway",
  "publishedIp": "192.0.2.10",
  "pkiDirectory": "pki",
  "requestTimeoutMs": 3000
}
```

Станки задаются в `config/machines.csv`:

```csv
Enabled,Id,Name,CypCutIp,CypCutPort,OpcUaPort,EndpointPath,PollIntervalMs,AppName
true,laser-01,Laser 01,192.0.2.101,8080,4880,/api/monitor/cutSystemState?ip={ip}&appName={appName},1000,CypCut
false,laser-02,Laser 02,192.0.2.102,8080,4881,/api/monitor/cutSystemState?ip={ip}&appName={appName},1000,CypCut
```

## Проверка из исходников

Требуется .NET 8 SDK:

```powershell
dotnet restore .\src\CypCutOpcUaGateway\CypCutOpcUaGateway.csproj
dotnet build .\src\CypCutOpcUaGateway\CypCutOpcUaGateway.csproj -c Release
dotnet run --project .\src\CypCutOpcUaGateway -- --self-test
dotnet run --project .\src\CypCutOpcUaGateway -- --validate-config
```

## Параметры

Шлюз создаёт 78 известных технологических параметров и 9 служебных узлов — всего
87 переменных на станок. Полный список: [docs/PARAMETERS-RU.md](docs/PARAMETERS-RU.md).

## Независимость проекта

Это неофициальная независимая интеграция. Проект не связан с разработчиком CypCut,
не является его продуктом и не использует его закрытый исходный код. Название
CypCut используется только для обозначения совместимости.

## Безопасность

Начальная конфигурация допускает анонимное OPC UA-подключение и `Security=None` для
стендовой проверки. Не публикуйте порты шлюза в интернет. Используйте изолированную
производственную сеть и сертификаты перед эксплуатацией.

Автор: **Viktor Matskevich** — Industrial AI, machine connectivity and intelligent
systems for the physical world.
