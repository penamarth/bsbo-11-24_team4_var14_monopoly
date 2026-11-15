# Монополия (консольная версия на C#)

Упрощённая консольная игра «Монополия» для 2–6 игроков на C# (`net8.0`).

## Правила игры

- Игроки начинают на «Старте» с балансом 1500. Порядок ходов и обе колоды перемешиваются при запуске партии.
- В обычном ходе бросаются два кубика один раз. Прохождение «Старта» приносит 200. Игрок в тюрьме пропускает три хода и может бросить кубики в следующий свой ход.
- При попадании на свободную собственность игра сначала проверяет баланс. Отказ или нехватка денег запускает аукцион среди игроков без банкротства.
- Аренда зачисляется владельцу клетки. Оплата покупки, победной ставки, строительства, налога и штрафа списывается без получателя.
- При нехватке денег на обязательный платёж все дома и отели должника продаются за полцены. Если средств всё ещё недостаточно, остаток денег и собственность переходят кредитору по аренде. При налоге или штрафе собственность освобождается. Когда остаётся один активный игрок, программа объявляет его победителем.

## Документация

- [Видение](<Видение.md>), [модель предметной области](<модель предметной области.md>) и [диаграмма классов](<система.puml>).
- [Начать новую игру](<прецеденты/Начать новую игру/Начать новую игру.md>).
- [Сыграть ход](<прецеденты/Сыграть ход/сыграть ход.md>).
- [Купить собственность / выставить на аукцион](<прецеденты/Купить собственность/Купить собственность_аукцион .md>).
- [Игрок становится банкротом](<прецеденты/Игрок становится банкротом (конец игры)/игрок становится банкротом.md>).

## Структура

```
Monopoly.sln
src/Monopoly/                 — сама игра (консольное приложение)
  Players/                    — Player, Dice, Auction, ввод (IInput, ConsoleInput)
  Squares/                    — клетки: Square, Property, Street, Railroad, Utility, Tax, ...
  Cards/                      — Card, Deck, EffectType
  Core/                       — Board, Game, GameStatus
  Program.cs                  — консольное меню
tests/Monopoly.Tests/         — unit-тесты (xUnit)
  Fakes/                      — FakeInput, FixedDice, TestGame (заготовки для тестов)
```

## Установка на Mac

1. Установить .NET 8 SDK:
   - через Homebrew: `brew install --cask dotnet-sdk`
   - или скачать установщик для macOS с https://dotnet.microsoft.com/download/dotnet/8.0
     (для Apple M1/M2/M3 — версия **Arm64**, для Intel — **x64**)
2. Проверить: `dotnet --version` (должно быть 8.x).
3. Редактор на выбор:
   - **JetBrains Rider** (бесплатный для некоммерческого использования) — просто открыть `Monopoly.sln`;
   - **VS Code** + расширение **C# Dev Kit** — открыть папку проекта.

## Запуск

Из корня проекта:

```bash
dotnet run --project src/Monopoly
```

Введите количество игроков и их имена. После запуска партии доступны пункты меню.

| Пункт | Действие |
|---|---|
| `1` | Сыграть ход. |
| `2` | Выбрать улицу для строительства. |
| `3` | Показать владения текущего игрока. |
| `4` | Показать всех игроков. |
| `0` | Прервать игру. |

## Тесты

```bash
dotnet test
```

### Покрытие кода

```bash
dotnet test --collect:"XPlat Code Coverage"

# один раз установить генератор отчёта
dotnet tool install -g dotnet-reportgenerator-globaltool

reportgenerator -reports:"tests/**/coverage.cobertura.xml" -targetdir:coveragereport -reporttypes:Html
open coveragereport/index.html
```

`Program.cs` и `ConsoleInput.cs` исключены из покрытия (`[ExcludeFromCodeCoverage]`):
это работа с консолью, её не тестируем.

## Разделение работы

| Часть | Что делает | Файлы игры | Тесты |
|---|---|---|---|
| **1. Игрок, кубики, аукцион** | настройка проекта, игрок, кубики, аукцион, ввод | `Monopoly.sln`, оба `.csproj`, `.gitignore`, `Players/*` | `Fakes/*`, `PlayerTests`, `DiceTests`, `AuctionTests` |
| **2. Собственность** | базовая клетка, покупка/аренда, улицы, ж/д, коммуналка | `Squares/Square`, `Property`, `Street`, `Railroad`, `Utility` | `PropertyTests`, `StreetTests`, `RailroadAndUtilityTests` |
| **3. Спец. клетки, карточки, доска** | налог, шанс, казна, тюрьма и т.д., карточки, колоды, доска из 40 клеток | `Squares/Tax`, `Chance`, `CommunityChest`, `Go`, `Jail`, `FreeParking`, `GoToJail`, `Cards/*`, `Core/Board` | `SpecialSquaresTests`, `CardTests`, `DeckTests`, `BoardTests` |
| **4. Игра и консоль** | ход игрока, покупка, аукцион, оплата, банкротство, строительство, меню | `Core/Game`, `Core/GameStatus`, `Program.cs`, `README.md` | `GameTests` |

## Как работаем с GitHub

Классы ссылаются друг на друга (клетка вызывает методы `Game`, а `Game` хранит клетки),
поэтому проект полностью собирается только когда залиты все 4 части.

1. **Участник 1** создаёт репозиторий и заливает свою часть в `main`:
   ```bash
   git init
   git add .
   git commit -m "Настройка проекта: игрок, кубики, аукцион"
   git branch -M main
   git remote add origin https://github.com/<user>/monopoly.git
   git push -u origin main
   ```
   Остальных добавляет в Settings → Collaborators.
2. **Участники 2, 3, 4** клонируют репозиторий и работают в своей ветке:
   ```bash
   git clone https://github.com/<user>/monopoly.git
   cd monopoly
   git checkout -b part-2-property        # part-3-squares-cards / part-4-game
   # добавить свои файлы
   git add .
   git commit -m "Собственность: улицы, ж/д, коммуналка + тесты"
   git push -u origin part-2-property
   ```
   Затем на GitHub открывают Pull Request в `main`.
3. PR сливаются по порядку: 2 → 3 → 4. После последнего проверяем:
   ```bash
   git pull
   dotnet build
   dotnet test
   ```
