# Инструкция по настройке Git репозитория

## Шаг 1: Инициализация локального репозитория

```bash
git init
```

## Шаг 2: Добавление всех файлов в staging

```bash
git add .
```

## Шаг 3: Создание первого коммита

```bash
git commit -m "Initial commit: Tracer project"
```

## Шаг 4: Переименование ветки в main (если нужно)

```bash
git branch -M main
```

## Шаг 5: Добавление удаленного репозитория

```bash
git remote add origin https://github.com/Elisabeth0321/CSharpProjects.git
```

## Шаг 6: Проверка удаленного репозитория

```bash
git remote -v
```

## Шаг 7: Получение изменений с удаленного репозитория (если там уже есть файлы)

```bash
git pull origin main --allow-unrelated-histories
```

Если возникнут конфликты, разрешите их и выполните:
```bash
git add .
git commit -m "Merge remote repository"
```

## Шаг 8: Отправка кода на GitHub

```bash
git push -u origin main
```

---

## Альтернативный вариант (если в удаленном репозитории уже есть файлы и вы хотите их перезаписать)

Если удаленный репозиторий содержит файлы (например, .gitignore), и вы хотите заменить их вашим кодом:

```bash
git push -u origin main --force
```

**⚠️ Внимание:** `--force` перезапишет все файлы в удаленном репозитории!

---

## Полный список команд (копировать и выполнять по порядку)

```bash
# 1. Инициализация
git init

# 2. Добавление файлов
git add .

# 3. Первый коммит
git commit -m "Initial commit: Tracer project"

# 4. Переименование ветки
git branch -M main

# 5. Добавление remote
git remote add origin https://github.com/Elisabeth0321/CSharpProjects.git

# 6. Проверка
git remote -v

# 7. Pull (если в удаленном репо есть файлы)
git pull origin main --allow-unrelated-histories

# 8. Push
git push -u origin main
```

---

## Если нужно удалить существующий remote и добавить заново

```bash
git remote remove origin
git remote add origin https://github.com/Elisabeth0321/CSharpProjects.git
```

---

## Проверка статуса

```bash
git status
```

## Просмотр истории коммитов

```bash
git log --oneline
```

