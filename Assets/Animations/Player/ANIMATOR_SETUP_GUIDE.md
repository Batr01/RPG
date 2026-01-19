# Инструкция по настройке Animator Controller для персонажа

## Шаг 1: Создание Animator Controller

1. В Unity перейдите в папку `Assets/Animations/Player/`
2. Правый клик → Create → Animator Controller
3. Назовите его `PlayerAnimator`

## Шаг 2: Настройка параметров

Откройте окно Animator (Window → Animation → Animator) и добавьте следующие параметры:

### Параметры:
- **Speed** (Float) - скорость движения персонажа (0-1)
- **IsMoving** (Bool) - флаг движения
- **Attack** (Trigger) - триггер для активации атаки
- **ComboIndex** (Int) - индекс текущей атаки в комбо (0, 1, 2)

## Шаг 3: Импорт анимаций

Анимации уже есть в проекте из пакета Kevin Iglesias:

### Путь к анимациям:
`Assets/Kevin Iglesias/Human Animations/Animations/Male/`

### Нужные анимации:
- **Idle**: `Idles/HumanM@Idle01`
- **Движение**: `Movement/Run/HumanM@Run01_Forward` (используем одну анимацию, поворот через скрипт)
- **Атаки**: `Combat/1H/HumanM@Attack1H01_R` (использовать для всех трёх ударов комбо)

## Шаг 4: Создание состояний (States)

### 4.1 Idle State
1. Перетащите анимацию `HumanM@Idle01` в окно Animator
2. Это состояние автоматически станет начальным (оранжевый цвет)
3. Назовите его `Idle`

### 4.2 Movement State (Простой подход)
**Стандартный подход - одна анимация бега, поворот через скрипт:**

1. Создайте обычное состояние `Movement`
2. Добавьте анимацию `HumanM@Run01_Forward`
3. Персонаж будет автоматически поворачиваться в сторону движения через скрипт `PlayerMovement`
4. Это проще и работает отлично для top-down вида

**Почему одна анимация?**
- Персонаж поворачивается через `transform.rotation` в скрипте
- Для top-down вида это идеально - видим персонажа сверху
- Не нужно настраивать Blend Tree
- Проще и стандартный подход

### 4.3 Attack States (Комбо)
Создайте три состояния атаки:

**Attack1:**
1. Создайте новое состояние
2. Назовите `Attack1`
3. Добавьте анимацию `HumanM@Attack1H01_R`
4. В Inspector добавьте Tag: `Attack`

**Attack2:**
1. Создайте новое состояние
2. Назовите `Attack2`
3. Добавьте ту же анимацию `HumanM@Attack1H01_R`
4. В Inspector измените Speed: `1.1` (чуть быстрее)
5. Добавьте Tag: `Attack`

**Attack3:**
1. Создайте новое состояние
2. Назовите `Attack3`
3. Добавьте ту же анимацию `HumanM@Attack1H01_R`
4. В Inspector измените Speed: `1.2` (еще быстрее)
5. Добавьте Tag: `Attack`

## Шаг 5: Настройка переходов (Transitions)

### Idle ↔ Movement
**Idle → Movement:**
- Условие: `IsMoving` = true
- Has Exit Time: `false`
- Transition Duration: `0.1`

**Movement → Idle:**
- Условие: `IsMoving` = false
- Has Exit Time: `false`
- Transition Duration: `0.1`

### Переходы атак из любого состояния (Any State)
Используйте **Any State** для запуска атак:

**Any State → Attack1:**
- Условие: `Attack` (trigger) AND `ComboIndex == 0`
- Has Exit Time: `false`
- Transition Duration: `0.05`

**Attack1 → Attack2:**
- Условие: `Attack` (trigger) AND `ComboIndex == 1`
- Has Exit Time: `false`
- Transition Duration: `0.05`
- Settings: Can Transition To Self: `false`

**Attack2 → Attack3:**
- Условие: `Attack` (trigger) AND `ComboIndex == 2`
- Has Exit Time: `false`
- Transition Duration: `0.05`
- Settings: Can Transition To Self: `false`

### Возврат к Idle после атак
**Attack1 → Idle:**
- Без условий
- Has Exit Time: `true`
- Exit Time: `0.9`
- Transition Duration: `0.1`

**Attack2 → Idle:**
- Без условий
- Has Exit Time: `true`
- Exit Time: `0.9`
- Transition Duration: `0.1`

**Attack3 → Idle:**
- Без условий
- Has Exit Time: `true`
- Exit Time: `0.9`
- Transition Duration: `0.1`

## Шаг 6: Оптимизация для мобильных устройств

В настройках Animator Controller:
1. Culling Mode: `Based On Renderers`
2. Update Mode: `Normal`
3. Можно отключить Root Motion если используете скриптовое управление

## Шаг 7: Применение к персонажу

1. Выберите модель персонажа `HumanM_Model` в сцене
2. Добавьте компонент `Animator`
3. В поле `Controller` перетащите созданный `PlayerAnimator`
4. В поле `Avatar` выберите Avatar из модели (должен создаться автоматически)
5. Убедитесь, что Apply Root Motion = `false` (управление через скрипт)

## Важные заметки

**Поворот персонажа:**
- Персонаж автоматически поворачивается в сторону движения через `PlayerMovement.RotateTowardsMovement()`
- Используется плавный поворот (Slerp) для плавности
- Не нужно настраивать отдельные анимации для разных направлений

## Проверка работоспособности

После настройки запустите игру и проверьте:
- ✅ Idle проигрывается, когда персонаж стоит
- ✅ Анимация бега включается при движении джойстиком
- ✅ Атака запускается по нажатию кнопки
- ✅ Комбо работает (3 последовательных удара)
- ✅ Переходы плавные без рывков

## Возможные проблемы и решения

**Проблема:** Анимации не переключаются
- **Решение:** Проверьте, что все параметры созданы правильно (Case-sensitive!)

**Проблема:** Персонаж "скользит"
- **Решение:** Убедитесь, что Apply Root Motion выключен

**Проблема:** Комбо не работает
- **Решение:** Проверьте условия переходов и Tag "Attack" на состояниях атак

**Проблема:** Анимации слишком быстрые/медленные
- **Решение:** Измените параметр Speed в Inspector для каждого состояния
