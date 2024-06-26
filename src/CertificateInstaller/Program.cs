/*
	Простая консольная программа для установки сертификатов с автоматическим подтверждением всплывающих окон безопасности.

	Данная программа является свободным программным обеспечением, распространяющимся по лицензии MIT.
	Копия лицензии: https://opensource.org/licenses/MIT

	Copyright (c) 2024 Otto
	Автор: Otto
	Версия: 26.06.24
	GitHub страница:  https://github.com/Otto17/CertificateInstaller
	GitFlic страница: https://gitflic.ru/project/otto/certificateinstaller

	г. Омск 2024
*/


using System;                                           // Библиотека предоставляет доступ к базовым классам и функциональности .NET Framework
using System.Runtime.InteropServices;                   // Библиотека предоставляет средства для взаимодействия с нативным кодом в C#
using System.Security.Cryptography.X509Certificates;    // Библиотека предоставляет возможности для работы с сертификатами X.509
using System.Threading;                                 // Библиотека предоставляет средства для работы с потоками исполнения в C#

namespace CertificateInstaller
{
    class Program
    {
        // Импорт необходимых функций WinAPI из библиотеки "user32.dll" (для работы с системными всплывающими окнами)
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);   // Используем метод "FindWindow()" для поиска окна по заданным параметрам

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetForegroundWindow(IntPtr hWnd);    // Используем метод "SetForegroundWindow()" для установки указанного окна на передний план

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);    // Используем метод "PostMessage()" для отправки сообщения указанному окну

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool BlockInput(bool fBlockIt);   // Используем метод "BlockInput()" для блокировки или разблокировки ввода пользователей (мышь/клавиатура)

        //Константы событий для клавиш
        private const uint WM_KEYDOWN = 0x0100; // Событие нажатия клавиши
        private const uint WM_KEYUP = 0x0101;   // Событие отпускания клавиши
        private const int VK_TAB = 0x09;        // Клавиша "Tab" на клавиатуре
        private const int VK_RETURN = 0x0D;     // Клавиша "Enter" на клавиатуре

        private static bool keepRunning = true; // Флаг для запуска/остановки нового потока

        static void Main(string[] args)
        {
            if (args.Length < 3)    // Если получили меньше 3-х аргументов, тогда выводим справку
            {
                Console.ForegroundColor = ConsoleColor.White; // Устанавливаем белый цвет для строк ниже
                Console.WriteLine("Установка всей цепочки сертификатов (* - не обязательный параметр):\n");
                Console.WriteLine("CertificateInstaller.exe <CurrentUser или LocalMachine> <Название хранилища> <Путь к сертификату> <*Пароль сертификата> <*Пометить этот ключ как экспортируемый (по умолчанию - false)> <*Включить все расширенные свойства (по умолчанию - true)>\n");

                Console.ForegroundColor = ConsoleColor.DarkGreen; // Устанавливаем тёмно-зелёный цвет для строк ниже
                Console.WriteLine("Поддерживается 6 типов сертификатов: *.pfx, *.p12, *.cer, *.crt, *.spc, *.p7b\n");
                Console.WriteLine("Поддерживается автоматическое подтверждение всплывающих окон при установке сертификата в \"Root\" текущего пользователя.");
                Console.WriteLine("Для корректного подтверждения всех всплывающих окон производится блокировка мыши и клавиатуры.\n");

                Console.ForegroundColor = ConsoleColor.Blue; // Устанавливаем синий цвет для строк ниже
                Console.WriteLine("Примеры:");
                Console.WriteLine("CertificateInstaller \"CurrentUser\" \"My\" \"cert.pfx\" \"pass123\"");
                Console.WriteLine("CertificateInstaller \"LocalMachine\" \"Root\" \"cert.p12\" \"pass123\" \"true\" \"true\"");
                Console.WriteLine("CertificateInstaller \"LocalMachine\" \"CA\" \"cert.cer\" \"false\" \"false\"");
                Console.WriteLine("CertificateInstaller \"CurrentUser\" \"TrustedPeople\" \"cert.pb7\" \"true\" \"false\"\n");

                Console.ForegroundColor = ConsoleColor.Red; // Устанавливаем красный цвет для строки ниже
                Console.WriteLine("Для блокировки мыши и клавиатуры (во время подтверждения всплывающих окон безопасности) программа должна быть запущена с правами администратора!\n");
                Console.ResetColor(); // Сбрасываем цвет на стандартный

                Console.WriteLine("Список названий хранилищ сертификатов:");
                Console.WriteLine("\"My\"               - Личные");
                Console.WriteLine("\"Root\"             - Доверенные корневые центры сертификации");
                Console.WriteLine("\"Trust\"            - Доверительные отношения в предприятии");
                Console.WriteLine("\"CA\"               - Промежуточные центры сертификации");
                Console.WriteLine("\"TrustedPublisher\" - Доверенные издатели");
                Console.WriteLine("\"AuthRoot\"         - Сторонние корневые центры сертификации");
                Console.WriteLine("\"TrustedPeople\"    - Доверенные лица");
                Console.WriteLine("\"AddressBook\"      - Другие пользователи\n");

                Console.ForegroundColor = ConsoleColor.Yellow; // Устанавливаем жёлтый цвет для строк ниже
                Console.WriteLine("Автор Otto, г.Омск 2024");
                Console.WriteLine("GitHub страница:  https://github.com/Otto17/CertificateInstaller");
                Console.WriteLine("GitFlic страница: https://gitflic.ru/project/otto/certificateinstaller");
                Console.ResetColor(); // Сбрасываем цвет на стандартный
                return;
            }

            //Массивы для получения аргументов
            string location = args[0];                                              // Получаем первый аргумент
            string storeName = args[1];                                             // Получаем второй аргумент
            string certPath = args[2];                                              // Получаем третий аргумент
            string certPassword = args.Length > 3 ? args[3] : null;                 // Получаем четвёртый аргумент, если длина массива больше 3, иначе присваивается "null"
            bool exportable = args.Length > 4 ? bool.Parse(args[4]) : false;        // Получаем пятый аргумент, преобразованное его в тип "bool", если длина массива больше 4, иначе присваивается "false"
            bool includeProperties = args.Length > 5 ? bool.Parse(args[5]) : true;  // Получаем шестой аргумент, преобразованное его в тип "bool", если длина массива больше 5, иначе присваивается "true"

            StoreLocation storeLocation;    // Переменная для выбора хранилища установки сертификата

            // Выбор хранилища в зависимости от указания первого аргумента
            if (location.Equals("CurrentUser", StringComparison.OrdinalIgnoreCase))
            {
                storeLocation = StoreLocation.CurrentUser;
            }
            else if (location.Equals("LocalMachine", StringComparison.OrdinalIgnoreCase))
            {
                storeLocation = StoreLocation.LocalMachine;
            }
            else
            {
                Console.WriteLine("Ошибка: неверное значение для параметра, выберите \"CurrentUser\" или \"LocalMachine\".");   // Выводим ошибку и завершаем работу программы
                return;
            }

            try
            {
                //Создание и запуск нового потока для автоматического подтверждения диалогового окна безопасности
                Thread promptHandlerThread = new Thread(AutoConfirmDialog);
                promptHandlerThread.Start();

                //Вызываем метод для установки сертификата с заданными параметрами
                InstallCertificate(certPath, certPassword, storeName, storeLocation, exportable, includeProperties);
                Console.WriteLine($"Сертификат успешно установлен в хранилище {location}.");

                //Остановка потока подтверждения
                keepRunning = false;        // Опускаем флаг для остановки потока
                promptHandlerThread.Join(); // Ожидаем завершения работы потока
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при установке сертификата: {ex.Message}");
                keepRunning = false;    // Опускаем флаг для остановки потока
            }
        }

        private static void InstallCertificate(string certPath, string certPassword, string storeName, StoreLocation storeLocation, bool exportable, bool includeProperties)
        {
            //Установка флагов для загрузки сертификатов
            X509KeyStorageFlags flags = X509KeyStorageFlags.PersistKeySet;  // Переменная "flag" типа "X509KeyStorageFlags"

            if (exportable) // Если флаг метки ключа как экспортируемого поднят
            {
                flags |= X509KeyStorageFlags.Exportable;    // Добавляем флаг к переменной
            }
            if (includeProperties)  // Если флаг метки включения всех расширенных свойств сертификата поднят
            {
                flags |= X509KeyStorageFlags.UserKeySet;    // Добавляем флаг к переменной
            }

            //Определение типа файла сертификата
            string extension = System.IO.Path.GetExtension(certPath).ToLower();         // Определяется тип файла сертификата по расширению "certPath" и приводится к нижнему регистру
            X509Certificate2Collection certificates = new X509Certificate2Collection(); // Создаётся коллекция "certificates" типа "X509Certificate2Collection"

            switch (extension)  // Проверяется тип расширения файла сертификата
            {
                //"*.pfx" и "*.p12" - обрабатываются используя "certificates.Import"
                case ".pfx":
                case ".p12":
                    if (string.IsNullOrEmpty(certPassword))
                    {
                        certificates.Import(certPath, "", flags);
                    }
                    else
                    {
                        certificates.Import(certPath, certPassword, flags);
                    }
                    break;

                //"*.cer", "*.crt", "*.spc" - загружаются как "X509Certificate2" и добавляются в коллекцию
                case ".cer":
                case ".crt":
                case ".spc":
                    X509Certificate2 cert = new X509Certificate2(certPath);
                    certificates.Add(cert);
                    break;

                //"*.p7b" - импортируется как коллекция сертификатов
                case ".p7b":
                    certificates.Import(certPath);
                    break;

                default:
                    throw new InvalidOperationException("Неподдерживаемый формат сертификата!");
            }

            //Открытие хранилища сертификатов
            X509Store store = new X509Store(storeName, storeLocation);  // Создаём новое хранилище сертификатов "X509Store" с указанным именем и расположением
            store.Open(OpenFlags.ReadWrite);                            // Открываем хранилище для чтения и записи

            foreach (X509Certificate2 cert in certificates)
            {
                store.Add(cert);    // Добавление всех сертификатов в хранилище
            }
            store.Close();  // Закрываем хранилище
        }


        //Поиск всплывающего окна для подтверждения установки цепочки сертификатов
        private static IntPtr FindSecurityWarningWindow()
        {
            IntPtr hwnd = FindWindow(null, "Предупреждение системы безопасности");  // Ищем окно по заголовку
            if (hwnd == IntPtr.Zero)    // Если окно не было найдено
            {
                hwnd = FindWindow(null, "Security Warning");    // Повторяем поиск, но уже на Английском языке (если система не Русифицирована)
            }
            return hwnd;
        }


        //Автоматическое подтверждение при установке цепочки сертификатов во всех всплывающих окнах (независимо сколько их будет)
        private static void AutoConfirmDialog()
        {
            while (keepRunning) // Если новый поток запущен
            {
                IntPtr hwnd = FindSecurityWarningWindow();  // Ищем окна из данного метода 
                if (hwnd != IntPtr.Zero)                    // Если окно нашли
                {
                    BlockInput(true); // Блокируем ввод с клавиатуры и мыши

                    try
                    {
                        Console.WriteLine("Найдено всплывающее диалоговое окно. Блокируем ввод и подтверждаем...");
                        SetForegroundWindow(hwnd);  // Помечаем найденное окно как активное

                        //Отправка клавиши "Tab" для перехода на кнопку "ДА" в диалоговом окне
                        PostMessage(hwnd, WM_KEYDOWN, (IntPtr)VK_TAB, IntPtr.Zero);
                        PostMessage(hwnd, WM_KEYUP, (IntPtr)VK_TAB, IntPtr.Zero);

                        Thread.Sleep(100); // Небольшая задержка для обработки действия

                        //Отправка клавиши "Enter" для подтверждения диалогового окна
                        PostMessage(hwnd, WM_KEYDOWN, (IntPtr)VK_RETURN, IntPtr.Zero);
                        PostMessage(hwnd, WM_KEYUP, (IntPtr)VK_RETURN, IntPtr.Zero);

                        Thread.Sleep(400); // Небольшая задержка для обработки действия
                    }
                    finally
                    {
                        BlockInput(false); // Разблокируем ввод с клавиатуры и мыши
                    }
                }
                Thread.Sleep(500); // Проверка цикла на новые окна каждые 500 мс, пока "InstallCertificate()" не завершит свою работу по установке цепочки сертификатов
            }
        }
    }
}
