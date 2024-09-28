**CertificateInstaller** - простая консольная программа для установки сертификатов в Windows с автоматическим подтверждением всплывающих окон безопасности, тем самым полностью автоматизировав установку сертификата без ручных подтверждений.

Программа может работать как как самостоятельно, так и совместно с другим моим проектом DAVrun, ссылка на [GitHub](https://github.com/Otto17/DAVrun) и [GitFlic](https://gitflic.ru/project/otto/davrun).

_Данная программа является свободным программным обеспечением, распространяющимся по лицензии MIT._

---

_**Что может данная утилита:**_

*   Поддержка 6 типов сертификатов: **\*.pfx, \*.p12, \*.cer, \*.crt, \*.spc, \*.p7b**, а так же установку всех цепочек сертификатов, которые могут содержаться в основном сертификате (_протестировал на всех видах сертификатов_).
*   Поддерживается автоматическое подтверждение всплывающих окон при установке сертификата в “Доверенные корневые центры сертификации” (Root) текущего пользователя.
*   Для корректного подтверждения всех всплывающих окон производится блокировка мыши и клавиатуры.
*   P.S. Для блокировки мыши и клавиатуры (во время подтверждения всплывающих окон безопасности) **программа должна быть запущена с правами администратора**!
*   Возможность пометить ключ сертификата как экспортируемый (по умолчанию **выключено**), а так же выключить все расширенные свойства сертификата (по умолчанию **включено**).

---

### Для блокировки мыши и клавиатуры (во время подтверждения всплывающих окон безопасности) программа должна быть запущена с правами администратора!

---

_**Аргументы (\* - не обязательный параметр):**_

```plaintext
CertificateInstaller.exe <CurrentUser или LocalMachine> <Название хранилища> <Путь к сертификату> <*Пароль сертификата> <*Пометить этот ключ как экспортируемый (по умолчанию - false)> <*Включить все расширенные свойства (по умолчанию - true)>
```

_**Примеры:**_

```plaintext
CertificateInstaller "CurrentUser" "Auto" "cert.pfx" "pass123"
CertificateInstaller "CurrentUser" "My" "cert.spc"
CertificateInstaller "LocalMachine" "Root" "cert.p12" "pass123" "true" "true"
CertificateInstaller "LocalMachine" "CA" "cert.cer"
CertificateInstaller "CurrentUser" "TrustedPeople" "cert.p7b"
```

_**Список названий хранилищ сертификатов:**_

```plaintext
"Auto"             - Автоматически выбрать хранилище на основе типа сертификата
"My"               - Личные
"Root"             - Доверенные корневые центры сертификации
"Trust"            - Доверительные отношения в предприятии
"CA"               - Промежуточные центры сертификации
"TrustedPublisher" - Доверенные издатели
"AuthRoot"         - Сторонние корневые центры сертификации
"TrustedPeople"    - Доверенные лица
"AddressBook"      - Другие пользователи
```

---

Для демонстрации запустите от имени администратора самораспаковывающийся архив “**setup.exe**” (созданный через WinRAR) из папки “**ТЕСТ**”.

Внутри сертификата “**TEST.pfx**” пять цепочек с сертификатами (_TEST1 - TEST5_), которые установятся в “**Доверенные корневые центры сертификации**”, так же есть **bat** файл для создания своих тестовых сертификатов.

---

### **Версии**

**27.09.2024г.**

*   Добавил в список названий хранилищ сертификатов ключ "**Auto**", который позволяет автоматически выбрать хранилище на основе типа сертификата для установки в "_Доверенные корневые центры сертификации_" или "_Личные_" как для одиночного сертификат, так и для цепочки сертификатов в **\*.pfx** _или_ **\*.p12** файле.

---

**Автор Otto, г. Омск 2024**