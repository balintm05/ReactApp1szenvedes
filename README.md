
# Premozi

A modern mozizás élményét fejlesztjük egy innovatív, felhasználóbarát online jegyfoglaló rendszerrel, amely lehetővé teszi a mozijegyek gyors, kényelmes foglalását. Projektünk célja, hogy a felhasználók könnyedén tallózhassanak a filmek és vetítések közt, kiválaszthassák kedvenc helyüket a teremben, majd lefoglalhassák a jegyeket – mindezt egy kényelmes, reszponzív, modern felületen.


## Fejlesztők

- [Bató Bence](https://www.github.com/ktjisebojtioeskotbwioi)
- [Magyarósi Bálint](https://github.com/balintm05)


## Telepítés

A projekt futtatásához szükséges programok:
- Visual Studio 2022
- XAMPP

A projekt klónozása után, nyissa meg a Visual Studio 2022-ben a terminált, majd a következő csomagokat telepítse, ha nincsenek vagy problémát okoznak:

```bash
  npm install react-router-dom
  npm install @emotion/react@11.14.0
  npm install @emotion/styled@11.14.0
  npm install @mui/material@7.0.1
  npm install axios@1.8.4
  npm install bootstrap@5.3.3
  npm install flowbite-react@0.10.2
  npm install jquery@3.7.1
  npm install popper.js@1.16.1
  npm install react-bootstrap@2.10.9
  npm install react-cookie@7.2.2
  npm install react-icons@5.5.0
  npm install react-native@0.78.0
  npm install react-router-dom@7.5.0
  npm install typescript-cookie@1.0.6
  npm install universal-cookie@7.2.2

```
Illetve a Visual Studio 2022-ben a következő NuGet csomagokat is telepítse, ha nincsenek vagy problémát okoznak:

```
BCrypt.Net-Next 4.0.3
Microsoft.AspNetCore.Authentication.JwtBearer 8.0.2
Microsoft.AspNetCore.Mvc.NewtonsoftJson 8.0.2
Microsoft.AspNetCore.SpaProxy 8.0.2
Microsoft.EntityFrameworkCore 8.0.2
Microsoft.EntityFrameworkCore.Design 8.0.2
Microsoft.EntityFrameworkCore.Tools 8.0.2
Microsoft.IdentityModel.JsonWebTokens 8.6.1
Microsoft.NET.Test.Sdk 17.13.0
Microsoft.VisualStudio.Web.CodeGeneration 8.0.2
Moq 4.20.72
MySql.Data 8.0.20
MySql.Data.EntityFramework 8.0.20
NUnit 4.3.2
NUnit3TestAdapter 5.0.0
Pomelo.EntityFrameworkCore.MySql 8.0.2
SendGrid 9.29.3
SixLabors.ImageSharp 3.1.7
Swashbuckle.AspNetCore 6.6.2
System.Linq.Async 6.0.1
xunit 2.9.3
xunit.runner.visualstudio 3.0.2

```

Ezután indítsa el a XAMPP-ot, lépjen be a [PhpMyAdmin](http://localhost/phpmyadmin)-ba és futtassa le a kódot a [premozi-dump-complete.sql](https://github.com/balintm05/Premozi-applikacio-projekt/blob/master/premozi-dump-complete.sql)-ből. 


## Helyi futtatás
Ha a program és a függőségek telepítése kész, indítsa el Visual Studio 2022-ben az F5 billentyű lenyomásával, vagy a https gomb megnyomásával.

A szerver projektnek kell kiválasztva lennie és a gombnak https-t kell írnia; ha nem indul el, akkor a teendő:


    -> https/start melletti leugró ablak 
    -> Configure Startup Projects 
    -> Multiple startup projects 
        -> felül: project: ReactApp.Server, action: Start, debug target: https
        -> alul: project: reactapp.client, action: Start, debug target: üres
    -> Alkalmaz 
    -> Current Selection 
    -> Alkalmaz

Ezután sikeresen el fog indulni a program.


Emellett egy indító BAT fájl is mellékelve van, de annak működése a tanúsítványok miatt inkonzisztens, úgyhogy használata nem javasolt. 

Ha a program elindult, másolja ki a linket vagy a terminálból, vagy innét:
```
https://localhost:60769/
```

Megjegyzés: a leadási határidőig nem sikerült megoldani a web szerveres hostolást. Ha a bemutatásig ez meg lesz oldva, a program a [premozi.site](https://premozi.site/) linken keresztül lesz majd elérhető.

## Dokumentáció

[Word Dokumentáció](https://github.com/balintm05/Premozi-applikacio-projekt/raw/refs/heads/master/Premozi%20Dokument%C3%A1ci%C3%B3%20Word.docx)

[PDF Dokumentáció](https://github.com/balintm05/Premozi-applikacio-projekt/blob/master/Premozi%20Dokument%C3%A1ci%C3%B3%20PDF.pdf)


# Funkciók

### Felhasználói felület
- Regisztráció és bejelentkezés – Biztonságos fiókkezelés kétlépcsős azonosítással, megerősítő emailekkel.
- Filmek böngészése – Áttekinthető lista a aktuális filmekről.
- Vetítések – Időpontok és a hozzájuk tartozó ülésrend megtekintése.
- Székválasztás- és foglalás – Interaktív térkép a moziteremről és a foglalások állapotáról valós időben.
- Foglaláskezelés - a felhasználók a vetítés kezdetéig bármikor visszavonhatják foglalásaikat.
- Email értesítések – Sikeres foglalásról és fontos információkról automatikus értesítés.

### Adminisztrációs felület
- Filmek, termek, vetítések kezelése – Új vetítések hozzáadása és meglévők szerkesztése.
- Székek konfigurálása – Moziterem elrendezés testreszabása.
- Felhasználók kezelése – Regisztrált felhasználók listája és admin jogosultságok.
- A szerveren tárolt képek kezelése.
- Foglalások törlése.


