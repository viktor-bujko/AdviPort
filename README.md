# AdviPort

AdviPort je jednoduchá aplikácia určená najmä pre ľudí, ktorí často využívajú leteckú 
dopravu, prípadne často trávia čas na rôznych letiskách. Cieľom aplikácie je totiž 
vytvoriť pomocníka, ktorý používateľom doručí rôzne informácie o vybranom letisku v 
reálnom čase.

Rozhranie, pomocou ktorého sa tento sprievodca letiskami ovláda je cez príkazový 
riadok. Aplikácia by mala obsahovať úvodnú stránku, slúžiacu ako menu na výber 
rôznych akcií, ktoré budú v aplikácii dostupné. 
Malo by ísť najmä o možnosť zaregistrovať sa do služby (podrobnejší popis nižšie), 
pridávať, odoberať a prehľadávať obľúbené letiská, vyhľadávať informácie o 
príletoch a odletoch (podľa dostupnosti informácií z jednotlivých letísk) či získavať 
rôzne iné dostupné informácie.

Ovládanie / vstup aplikácie ako aj jej výstup budú textové - použitie konkrétnych 
akcií či operácií by malo byť interaktívne. 

Získavanie dát v reálnom čase je založené na využití REST API služby / služieb, 
ktorá požadované informácie poskytuje. Na identifikáciu používateľa a prístup k 
dátam je však často potrebný API kľúč. Ten by bol používateľovi dodaný 
registráciou na stránke služby a následne používaný aj aplikáciou. 
Používateľ by svojou požiadavkou zostavil API endpoint pre službu, ktorej 
odpoveď (pravdepodobne vo formáte JSON) by bola spracovaná a 
štrukturovane vypísaná na konzolu. Na odosielanie jednotlivých požiadavok a 
prijímanie odpovedí zo servera sa predpokladá využitie http klienta.

Zápočtový program by mal obsahovať aj niektoré z konceptov preberaných počas letného 
semestra v rámci sylabu predmetu NPRG038 (asynchrónne metódy, generické metódy, 
delegáti / lambda funkcie).

 
