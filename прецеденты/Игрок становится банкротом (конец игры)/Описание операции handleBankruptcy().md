# UC4. Обязательный платёж и банкротство

**Действующее лицо** — система.

**Предусловие** — игрок должен заплатить аренду, налог или штраф по карточке. Процедура банкротства начинается, если его текущего баланса не хватает на обязательный платёж.

**Результат** — долг погашен либо игрок становится банкротом, а его имущество передаётся кредитору или освобождается.

## Системные операции

### `Game.ChargePayment(Player payer, Player? receiver, int amount)`

Проверяет баланс через `Player.CanAfford(amount)`. При достаточном балансе вызывает `payer.Pay(amount)` и, если указан получатель, `receiver.Receive(amount)`. При нехватке денег вызывает `Game.HandleBankruptcy(payer, receiver, amount)`.

### `Game.HandleBankruptcy(Player debtor, Player? creditor, int amount)`

Сначала вызывает `Game.SellAllBuildings(debtor)` и повторяет проверку баланса. Если средств хватило, списывает полную сумму и зачисляет её кредитору при его наличии. Иначе передаёт кредитору остаток денег и вызывает `Board.TransferProperties(debtor, creditor)`. При отсутствии кредитора вызывает `Board.ReleaseProperties(debtor)`. После распределения имущества вызывает `Player.DeclareBankrupt()` и `Game.CheckGameOver()`.

### `Game.SellAllBuildings(Player player)`

Получает улицы игрока через `Board.GetPropertiesOf(player)` и определяет уровень застройки каждой через `Street.GetBuildingLevel()`. Для положительного уровня зачисляет игроку сумму `level * street.HouseCost / 2` и вызывает `Street.ClearBuildings()`. Отель считается за пять домов.

### `Board.TransferProperties(Player from, Player to)` и `Board.ReleaseProperties(Player player)`

Оба метода находят собственность игрока по `Property.Owner`. При передаче каждой клетке назначается новый владелец через `Property.SetOwner(to)`. При освобождении для улиц сбрасывается застройка и у каждой клетки очищается владелец через `Property.SetOwner(null)`.

### `Player.DeclareBankrupt()`

Устанавливает `Bankrupt = true`, обнуляет `Cash` и снимает признак `InJail`. Игрок остаётся в `Game.Players`, но исключается из списка активных и пропускается при смене очереди.

### `Game.CheckGameOver()` и `Game.GetWinner()`

`CheckGameOver()` устанавливает статус `Finished` и возвращает `true`, если активных игроков осталось не больше одного. После обработки клетки `Game.PlayTurn()` также проверяет состояние партии и вызывает `Game.AdvanceToNextPlayer()`. Когда статус уже `Finished`, индекс не меняется. После выхода из игрового цикла `Program` вызывает `GetWinner()` и выводит имя оставшегося активного игрока, если он есть.

[Диаграмма последовательности](<Диаграмма последовательности Игрок становится банкротом.puml>)
