-- Добавляем тестовые случаи для задачи сортировки
INSERT INTO "TestCases" ("CaseId", "Input", "ExpectedOutput", "Description")
SELECT 
    c."Id",
    '[5, 2, 9, 1, 5, 6]',
    '[1, 2, 5, 5, 6, 9]',
    'Обычный случай с повторяющимися элементами'
FROM "Cases" c
WHERE c."Title" = 'Оптимизация алгоритма сортировки'
UNION ALL
SELECT 
    c."Id",
    '[1, 2, 3, 4, 5]',
    '[1, 2, 3, 4, 5]',
    'Уже отсортированный массив'
FROM "Cases" c
WHERE c."Title" = 'Оптимизация алгоритма сортировки'
UNION ALL
SELECT 
    c."Id",
    '[5, 4, 3, 2, 1]',
    '[1, 2, 3, 4, 5]',
    'Массив в обратном порядке'
FROM "Cases" c
WHERE c."Title" = 'Оптимизация алгоритма сортировки'
UNION ALL
SELECT 
    c."Id",
    '[1]',
    '[1]',
    'Массив из одного элемента'
FROM "Cases" c
WHERE c."Title" = 'Оптимизация алгоритма сортировки'
UNION ALL
SELECT 
    c."Id",
    '[]',
    '[]',
    'Пустой массив'
FROM "Cases" c
WHERE c."Title" = 'Оптимизация алгоритма сортировки'; 