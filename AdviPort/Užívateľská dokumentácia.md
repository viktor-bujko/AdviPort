# Užívateľská dokumentácia

## Inštalácia programu

Program `AdviPort`  je dostupný ako repozitár na [GitHub-e](https://github.com/viktor-bujko/AdviPort). Na samotnú inštaláciu je potrebné si repozitár naklonovať

## Spustenie programu

Program sa spúšťa z konzoly.

## Ovládanie a práca s prostredím programu

`AdviPort` je program, ktorý je ovládaný interaktívne, textovou formou. 

- Výber jednotlivých dostupných akcií v **hlavnom menu** je možné realizovať nasledujúcimi spôsobmi:

	- ### Výber operácie zadaním poradového čísla

    ​	Ľubovoľnú dostupnú operáciu je možné vyvolať zadaním jej poradového čísla. Každá operácia má priamo na obrazovke vypísané poradové číslo, ktoré jej je priradené a na základe ktorého je možné operáciu jednoznačne identifikovať. Pri tomto spôsobe ovládania je potrebné zadať iba konkrétne poradové číslo bez akýchkoľvek iných znakov. Program akceptuje iba čísla, ktoré priamo zodpovedajú jednotlivým operáciám - teda číslam od 1 až po počet práve dostupných operácií. Pri zadaní iného, nesprávneho poradového čísla je najskôr používateľ upozornený na chybu spolu s rozsahom korektných hodnôt a následne sa aplikácia znovu dostane do pôvodného stavu, v ktorom sa čaká na výber konkrétnej operácie.

	- ### Výber operácie zadaním prvého slova

    ​	Dostupné operácie je možné vyvolať aj zadaním <u>**prvého**</u> slova z názvu danej operácie, ktorý je dostupný priamo na obrazovke. Pri tomto spôsobe ovládania je potrebné zadať iba **prvé** slovo bez akýchkoľvek iných znakov, na veľkosti jednotlivých znakov však *nezáleží*. V prípade, že program obsahuje viacero operácií začínajúcich rovnakých prvým slovom je takýto spôsob výberu nejednoznačný a používateľ je na túto situáciu upozornený spolu s počtom rôznych operácií medzi ktorými konflikt nastal. Taktiež je pre vyriešenie konfliktu zadať poradové číslo operácie (ako bolo popísané v odstavci vyššie). Po upozornení sa aplikácia znovu dostane do pôvodného stavu, v ktorom sa čaká na výber konkrétnej operácie.

	- ### Výber pomocou šípok

    ​	Program v hlavnom menu podporuje aj ovládanie pomocou šípok. Pre výber konkrétnej operácie stačí pri tomto spôsobe ovládania použiť šípku dole / hore, pričom sa na obrazovke zobrazí číslo poradové číslo práve vybranej operácie. Výber pomocou šípok (rozsah poradových čísel) je obmedzený práve počtom aktuálne dostupných operácií. Potvrdenie a vyvolanie vybranej operácie sa uskutoční stlačením tlačidla `Enter`.  Všetky ostatné zadané znaky sú pri tomto spôsobe ovládania ignorované.
    
  
- ### Ovládanie jednotlivých operácií

  Každá z operácií môže definovať svoj vlastný spôsob ovládania, ktorý bude dostupný iba v konkrétnej operácii. V súčasnej verzii programu však program využíva iba zadávanie potrebných a nevyhnutných informácií pomocou textu. V prípade zadávania hesla je využité zadávanie textu, pri ktorom sa zadané znaky *nezobrazujú* na obrazovke. V ostatných prípadoch súčasná verzia podporuje klasický spôsob zadávania textu na konzolu.

## Súbory, s ktorými program pracuje

Program pracuje s viacerými textovými súbormi. 

- ### settings.json 

  ​	Na získanie informácií o všeobecných nastaveniach aplikácie akými je napríklad zoznam dostupných operácií či prednastavená hodnota vzhľadu hlavého menu slúží súbor `settings.json` ktorý sa nachádza v hlavnom adresári aplikácie. 

  **Upozornenie**: Zmena umiestnenia či názvu súboru môžu spôsobiť nesprávne fungovanie niektorých častí aplikácie.

- ### about.json

  ​	Tento textový súbor slúži najmä na poskytnutie základaných informácií o aplikácii, no zároveň obsahuje aj odkaz na špecifikáciu programu. Rovnako ako `settings.json` sa tento súbor nachádza v hlavnom adresári aplikácie.

  **Upozornenie**: Zmena umiestnenia či názvu súboru môžu spôsobiť nesprávne fungovanie niektorých častí aplikácie.
  
- ### súbory s príponou .apt

  V súčasnej verzii program používa databázu registrovaných používateľov, ktorá je založená na súborovom systéme. Pre každého registrovaného používateľa je vytvorený súbor s názvom `{meno_používateľa}_userprofile.apt` s umiestnením `{hlavný adresár aplikácie}/profiles/`. Pomocou tohto súboru je možné jednoznačne identifikovať registrovaného používateľa. Samotný súbor užívateľského profilu obsahuje základne údaje o používateľovi ako napr. jeho meno používateľa, programom zakodóvaný používaný API kľúč pre využívanie služieb, programom zakódované prihlasovacie heslo ale aj iné údaje, ktoré môže v aplikácii používateľ meniť (resp. ktoré sa menia používaním aplikácie) - zoznam svojich obľúbených uložených letísk, zoznam posledných vyhľadávaných letov ...

# Programátorská dokumentácia

## Hlavné triedy programu

## Náčrt architektúry

### Aplikačný životný cyklus

Beh aplikácie AdviPort sa dá charakterizovať hlavným životným cyklom:

- v prvom kroku sa na obrazovke zobrazí hlavné menu, ktorého vzhľad a obsah sa rozlišuje podľa toho, či je do aplikácie prihlásený nejaký registrovaný používateľ, prípadne podľa hlavných nastavení aplikácie.
- následne si používateľ jedným z [podporovaných spôsobov](#Výber-operácie-zadaním-poradového-čísla) vyberie operáciu, ktorú chce vykonať 
- na základe tohto používateľského vstupu program vyhodnotí, či je možné jednoznačne určiť operáciu (inštanciu triedy, ktorá implementuje aspoň rozhranie `IExecutablePlugin`), ktorá sa má vykonať
- pri kladnom vyhodnotení predchádzajúceho kroku takúto operáciu zavolá volaním metódy `int Invoke()`
  - návratová hodnota tejto metódy je číslo typu `int`, ktoré reprezentuje `exit code` - hodnotu indikujúcu (ne)správne dokončenie volanej operácie
- návrat z metódy `Invoke()` v metóde `Program.Main` predstavuje ukončenie konkrétnej operácie a vyhodnotenie návratovej hodnoty
- ukončenie aplikácie nastane v prípade, že bola zavolaná operácia `exit` a jej návratová hodnota je `0`.

### Architektúra zmeny vzhľadu úvodnej hlavnej stránky aplikácie

​	Na úpravu vzhľadu / doplnenie vzhľadu hlavnej stránky je použitý návrhový vzor `Decorator pattern`, ktorý rôznymi spôsobmi rozširuje 2 hlavné dostupné vzhľady - `classic` a `decorative` implementované v triedach `AdviPort.UI.ClassicMainPageHandler` resp. `AdviPort.UI.DecorativeMainPageHandler`.  Rozšíreniami vzhľadu je v aktuálnej verzii možné pridať údaje o prihlásenom používateľovi (pomocou triedy `AdviPort.UI.ShowLoggedUserMainPageHandler`) alebo zobraziť dodatočný popis dostupných operácií (pomocou triedy `AdviPort.UI.DescriptiveMainPageHandler`).
​	Hlavné triedy, ktoré reprezentujú vzhľad úvodnej stránky sú potomkami abstraktnej triedy `AdviPort.UI.AbstractMainPageHandler`. Táto trieda poskytuje rozhranie, ktoré by mali jednotlivé modifikátory vzhľadu hlavnej stránky implementovať - použitie triedy namiesto rozhrania je však podmienené definovaním predvolenej implementácie niektorých metód. Naviac, `AbstractMainPageHandler` implementuje rozhranie `IMainPageHandler`, ktoré rozširuje rozhrania `IMainPagePrinter` a `IUserInterfaceReader`  o možnosť spracovania výberu konkrétnej operácie ako aj vlastnosť poskytujúcu nadpis pre úvodnú obrazovku. `IMainPagePrinter` a `IUserInterfaceReader` sú rozhrania zodpovedné za komunikáciu s používateľom pomocou výpisu výstupu (ponuky dostupných operácií) a čítania užívateľského vstupu.

### Návrh dostupnosti jednotlivých operácií - trieda `AdviPort.Plugins.PluginSelector`

​	Výber dostupných operácií v súčasnej verzii prebieha pomocou fixne definovaných párov názov-operácia v metóde `static IPlugin GetPluginByName(string)`  statickej triedy `AdviPort.Plugins.PluginSelector`, kde názov je reťazec definovaný v súbore `settings.json` a operácia je inštancia triedy implementujúcej aspoň rozhranie `IPlugin` . Táto trieda taktiež poskytuje aj možnosť vyhľadávať a filtrovať dostupné operácie pomocou metódy `static IReadOnlyList<IPlugin> GetAvailablePlugins(GeneralApplicationSettings, Predicate<IPlugin>)`, ktorá berie do úvahy všeobecné nastavenia aplikácie na získavanie pluginov ako aj delegáta na predikát reprezentujúci filter jednotlivých operácií.

### Návrh hierarchie tried jednotlivých operácií

​	Každá spustiteľná operácia musí implementovať rozhranie `IExecutablePlugin` poskytujúce metódu `int Invoke()`. Rozšírením tohto rozhrania je hlavné rozhranie `IPlugin`, pomocou ktorého je možné konkrétnym operáciám priradiť meno a voliteľný rozširujúci popis. 
​	Rozhranie `IPlugin` sa následne rozdeľuje na rozhranie `ILoggedOffOnlyPlugin` - rozhranie, ktoré už kontrakt nerozširuje o žiadnu funkcionalitu, no poskytuje spôsob, akým je možné označiť operáciu tak, aby bola dostupná iba pre neprihlásených používateľov. Naopak, `LoggedInOnlyPlugin` je <u>abstraktná trieda</u>, ktorá povoľuje operáciu zobraziť a ponúknuť na použitie iba v prípade, že je používateľ prihlásený, resp. sa predpokladá, že bude využívať funkcie dostupné iba pre prihlásených používateľov.

### Architektúra poskytovania informácií pomocou REST API

​	Získavanie real-time informácií prebieha pomocou REST API endpointov, na ktoré môže <u>prihlásený</u> používateľ odosielať požiadavky. Aby bolo možné získavať rôzne informácie od rôznych poskytovateľov, program používa pre každý typ takejto informácie vlastný interface, pomocou ktorého je možné získanie konkrétneho typu informácie uskutočniť. Tieto rozhrania sú definované ako `I{názov poskytovaného objektu}Provider` , ktoré typicky požadujú implementáciu generickej asynchrónnej metódy vracajúcej `Task<T>`, ktorého typový argument je výsledný typ špecifikovaný pri volaní danej metódy rozhrania.
​	V súčasnej verzii sú implementácie týchto metód postavené na overení prihláseného používateľa s cieľom získať a dešifrovať jeho uložený API prístupový kľúč k službe. Následne je z požiadavok používateľa postavená adresa API endpoint-u, ktorý v prípade korektne zostavenej požiadavky takúto požiadavku spracuje a programu spätne odošle odpoveď s požadovanými informáciami.

### Definícia objektov pre (de)serializáciu JSON odpovedí

- Triedy / štruktúry definované v zdrojovom súbore `ResponseObjects.cs`
  - Štruktúry sú pre jednotlivé typy použité prednostne - najmä v prípade, že ide o zachovanie hodnotovej sémantiky bez potreby využitia iných schopností tried.
  - Triedy - použité pre typy `Flight`, `Airport`
    - pri triede `Flight` je využitá kovariancia *referenčného typu* v extension metódach statickej triedy `FlightListExtension`
    - typ `Airport` je definovaný ako trieda z dôvodu chýbajúcej podpory (de)serializácie interface-ov použitou knižnicou `JsonSerializer` - takáto (de)serializácia prebieha počas ukladania informácie o obľúbenom letisku konkrétneho používateľa do súboru reprezentujúceho jeho používateľský profil.

- Objekty, s ktorými program pracuje vrámci serializácie inštancií a deserializácie JSON odpovedí majú vlastnosti obsahujúce verejné, automaticky implementované gettery a settery ako aj prípadné metódy zabezpečujúce štruktúrovaný výpis takýchto objektov.  Pre každý takýto objekt je vytvorený interface popisujúci vlastnosti, ktoré sú na (de)serializáciu použité.

## Hlavné riešené problémy

- ### Podpora rôznych formátov API a (de)serializácia JSON odpovedí

  - Na (de)serializáciu jednotlivých objektov použitých v programe je použitá statická trieda štandardnej knižnice C# `JsonSerializer`. [Pravidlá deserializácie](https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-how-to?pivots=dotnet-core-3-1#deserialization-behavior) (ktorá je v programe používaná najčastejšie) výrazne ovplyvnili spôsob implementácie jednotlivých deskriptorov objektov. Trieda `JsonSerializer` vyžaduje a podporuje iba (de)serializáciu **verejných vlastností** obsahujúcich obe <u>**getter**</u> aj <u>**setter**</u>. Použitím tohto predvoleného správania bolo teda potrebné vytvoriť viaceré triedy spĺňajúce tieto pravidlá spolu s upresnením niektorých možností pomocou `JsonSerializerOptions`.
  - Kvôli rozličným API, ktorých formáty odpovedí sa navzájom s veľkou pravdepodobnosťou líšia, by bolo treba potrebné vytvoriť navzájom nekompatibilné deskriptory objektov pre každý formát API, s ktorým by program mal pracovať a následne tieto typy používať ako typové argumenty metód.
  - Ďalšou, vhodnejšou možnosťou je vytvorenie rozhrania deskriptoru, ktorý by obsahoval gettery a settery pre jednotlivé vlastnosti. Takýto prístup sa spolu s použitím triedy `JsonSerializer` dá použiť iba v prípade, že sa mená takýchto vlastností zhodujú s formátom JSON objektov, ktoré chceme (de)serializovať, alebo dodatočným implementovaním vlastných konvertorov pomocou triedy `JsonConverter<T>`.

  

- ### Rozhranie `ILoggedOffOnlyPlugin` vs. abstraktná trieda `LoggedInOnlyPlugin`

  ​	Rozdiel medzi rozličnými spôsobmi implementácie týchto 2 rozšírení kontraktu `IPlugin` spočíva v zamedzení dvojitej dedičnosti tried, ktoré chcú, aby ich funkcionalita bola dostupná pre oboch - prihlásených aj neprihlásených používateľov.
  ​	Ďalším dôvodom pre rôzne prístupy k definíciam týchto rozhraní je aj vynútenie si prihlásenia používateľa v prípade `LoggedInOnlyPlugin` pomocou predvolenej implementácie metódy, čo nie je možné definovať vrámci definície interface-u. Rozhranie `ILoggedOffOnlyPlugin` toto nevyžaduje, a preto bolo možné ho definovať pomocou interface-u.

  ​	Alternatívnym riešením, ktoré by zabezpečilo väčšiu konzistenciu v kóde je podobne ako pre `ILoggedOffOnlyPlugin` vytvorenie prázdneho označovacieho interface-u `ILoggedInOnlyPlugin`, ktorý by bol implementovaný abstraktnou triedou, v ktorej by bola predvolená funkcionalita zaručujúca prihlásenie používateľa implementovaná.

  

- ### Návrh a riešenie poskytovania real-time informácií

  ​	 **Alternatívnym riešením** namiesto viacerých rozhraní s podobným API metód by bolo vytvorenie 1 *všeobecného generického rozhrania*, ktorého typovým argumentom by bol typ, ktorého inštancia by mala vzniknúť ako výsledok volania. V tomto prípade by však bolo potrebné vytvoriť také API metódy daného interface-u, ktoré bude spoločné a postačujúce pre všetky získavané druhy informácií. Ďalším dôvodom prečo by takáto všeobecná implementácia nemusela byť dostatočná je, že rôzne druhy informácií môžu byť spracovávané rôznymi, na sebe navzájom nezávislými spôsobmi, čo môže spôsobiť nekompatibilitu vrámci implementácie metódy.

