﻿## RuCaptchaML

Для работы требуется установка OpenCV.

## RuCaptchaML.Train

Запускается для обучения ИИ.
Перед обучением скопировть подготовленные изображения в директорию inputImages.
Изображения должны иметь формат {расшифровка символов}.jpg или .png.

## RuCaptchaML.WebApp

Web приложение для распознания символов на изображении.
Перед запуском скопировать обученную модель (полученную при обучении, путь к моделе можно найти в конфигуррации) в директорию указанную в файле appsettings.json.
Имя модели должно совпадать с именем указанным в appsettings.json.