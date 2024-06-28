@echo off
setlocal enabledelayedexpansion

:: Создание корневого сертификата
openssl req -x509 -newkey rsa:2048 -keyout rootCA.key -out rootCA.crt -days 3650 -nodes -subj "/C=RU/ST=Russia/L=Omsk/O=TEST/OU=TEST/CN=TEST"

:: Создание промежуточных сертификатов
set prev=rootCA
for /L %%i in (2,1,5) do (
    openssl req -newkey rsa:2048 -keyout intermediate%%i.key -out intermediate%%i.csr -nodes -subj "/C=RU/ST=Russia/L=Omsk/O=TEST/OU=TEST/CN=TEST%%i"
    openssl x509 -req -in intermediate%%i.csr -CA !prev!.crt -CAkey !prev!.key -CAcreateserial -out intermediate%%i.crt -days 3650 -sha256
    set prev=intermediate%%i
)

:: Создание конечного сертификата
openssl req -newkey rsa:2048 -keyout leaf.key -out leaf.csr -nodes -subj "/C=RU/ST=Russia/L=Omsk/O=TEST/OU=TEST/CN=TEST6"
openssl x509 -req -in leaf.csr -CA intermediate5.crt -CAkey intermediate5.key -CAcreateserial -out leaf.crt -days 3650 -sha256

:: Объединение сертификатов в один файл
(
    type leaf.crt
    type intermediate5.crt
    type intermediate4.crt
    type intermediate3.crt
    type intermediate2.crt
    type rootCA.crt
) > fullchain.crt

:: Создание PFX-файла
openssl pkcs12 -export -out fullchain.pfx -inkey leaf.key -in leaf.crt -certfile fullchain.crt -name "MyCertificate" -passout pass:1
