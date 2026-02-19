# GorzdravParser

Консольное приложение на .NET для парсинга каталога товаров с сайта [gorzdrav.org](https://gorzdrav.org). Загружает страницы через Selenium, извлекает данные из JSON-ответов и сохраняет результат в CSV-файл.

---

## Возможности

- Загрузка страниц через Selenium
- Парсинг JSON-ответов API сайта
- Извлечение: названия, цены, цены без скидки, производителя, страны, действующего вещества, признака рецептурного отпуска, ссылки на товар и изображение
- Сохранение результата в CSV

---

## Требования

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Google Chrome + ChromeDriver, совместимый по версии

---

## Установка и запуск

```bash
git clone https://github.com/your-username/GorzdravParser.git
cd GorzdravParser
dotnet restore
dotnet run
```

---

## Конфигурация

Все настройки задаются в `appsettings.json` в секции `PageLoaderSettings`.

```json
{
  "PageLoaderSettings": {
    "RegionCodes": {
      "kaliningrad": "KGD",
      "spb": "SPE"
    },
    "Tasks": [
      {
        "BaseUrl": "https://gorzdrav.org/category/sredstva-ot-diabeta/",
        "OutputFileName": "result.csv"
      },
      {
        "BaseUrl": "https://gorzdrav.org/kaliningrad/category/sredstva-ot-diabeta/",
        "OutputFileName": "result_kaliningrad.csv"
      }
    ]
  }
}
```

### PageLoaderSettings

| Поле           | Тип              | Описание                                                                 |
|----------------|------------------|--------------------------------------------------------------------------|
| `RegionCodes`  | `object`         | Словарь соответствий города → код региона                                |
| `Tasks`        | `array`          | Список задач для последовательного выполнения                            |

### Tasks

| Поле             | Описание                                              |
|------------------|-------------------------------------------------------|
| `BaseUrl`        | URL категории для парсинга.                           |
| `OutputFileName` | Имя выходного CSV-файла                               |

### RegionCodes

Словарь позволяет сопоставить slug города из URL с кодом региона, который может требоваться в запросах к API горздрав. Ключ — slug из URL (`kaliningrad`, `spb`), значение — код региона (`KGD`, `SPE`).

Можно добавить любое количество задач и регионов — задачи выполняются последовательно.

---

## Структура проекта

```
GorzdravParser/
├── Configurators/          # Классы настроек (GorzdravSettings, LoadPageTask)
├── Interfaces/             # Контракты сервисов (IPageLoader, IParser, ICsvWritter)
├── Models/                 # Модели данных (Product)
├── Services/
│   ├── SeleniumPageLoader  # Загрузка HTML через Selenium
│   ├── GorzdravParser      # Парсинг JSON-ответов API
│   └── CsvWritter          # Запись результата в CSV
├── appsettings.json
└── Program.cs
```

---

## Формат CSV

Каждая строка соответствует одному товару. Поля:

| Поле               | Описание                                  |
|--------------------|-------------------------------------------|
| `Id`               | Идентификатор товара                      |
| `Name`             | Название                                  |
| `Prescription`     | Рецептурный отпуск (Да / Нет)             |
| `Manufacturer`     | Производитель                             |
| `ActiveIngredient` | Действующее вещество                      |
| `Price`            | Актуальная цена                           |
| `OldPrice`         | Цена без скидки (если есть)               |
| `ImageUrl`         | Ссылка на изображение                     |
| `ProductUrl`       | Ссылка на страницу товара                 |
| `Country`          | Страна производства                       |

---

## Как работает парсер

1. `SeleniumPageLoader` открывает браузер, переходит по `BaseUrl` и перехватывает XHR-ответы API, содержащие данные о товарах. Каждый перехваченный JSON сохраняется как отдельная строка в итоговой строке `rawData`.
2. `GorzdravParser` разбивает `rawData` по `\n` и для каждого блока десериализует JSON по пути `data → products → items`.
3. Из каждого элемента извлекаются поля товара, формируется абсолютная ссылка с учётом города из `BaseUrl`.
4. `CsvWritter` записывает список `Product` в указанный файл.

---
