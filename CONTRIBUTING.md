# Contribuer

## Compiler

Le SDK .NET 10 est en `/usr/lib/dotnet/dotnet`. Le `dotnet` du `PATH` peut pointer
sur une installation plus ancienne, qui ne voit pas `net10.0`.

```bash
npm ci
npm run css:minify
/usr/lib/dotnet/dotnet build --no-restore --nologo -v q
```

L'ordre compte. Le thème est compilé par Tailwind et **versionné** : c'est
`wwwroot/css/app.css`, produit par `npm run css:minify` à partir de
`wwwroot/css/design/theme.css`, que le navigateur reçoit. Modifier la source sans
recompiler ne change rien pour personne.

Pendant une séance de travail sur le style, `npm run css:watch` évite de relancer
la compilation à chaque modification.

## Ce que la compilation exige

Les avertissements sont des erreurs. Une compilation qui en produit un n'est pas
une compilation.

La documentation XML est obligatoire sur l'API publique — `<param>`, `<returns>`,
et une remarque qui explique **pourquoi** quand la raison ne se lit pas dans le
code. Les questions sans réponse ne sont pas des remarques.

## Langues

Le domaine, l'interface et les messages d'erreur sont en français. Les noms de
champs des contrats d'API sont en anglais : c'est la convention du projet, et la
changer coûterait plus qu'elle ne rapporterait.

## Choses qui coûtent cher quand on les oublie

**Une requête LINQ traduisible.** Le fournisseur SQL refuse certaines opérations —
un `OrderBy` assorti d'un comparateur, par exemple — et son refus fait échouer la
page entière, pas seulement la ligne. Matérialiser avant de trier.

**Un nom de membre dans une classe Razor.** Un composant ne peut pas avoir un
membre du même nom que sa classe : `Evenements` et `Ressources` s'appellent
`Service` et `Liste`.

**Les guillemets dans un attribut Razor.** Un `"` imbriqué casse l'analyse de la
page. Une apostrophe devient une constante de caractère : pour un enums dans un
attribut, il faut une enum nommée.

**Une chaîne de JS passée à `InvokeAsync<T>`.** Elle est interprétée comme un nom
de fonction. Pour tester la présence d'un objet, passer par
`DnAdminScripts.present`, pas par une expression.

## Conventions de commit

Un seul sujet, au présent de l'indicatif, préfixé du type :

```
feat: ajoute la page des partenaires
fix: rend la migration des articles idempotente
```

Le corps explique **pourquoi**, pas ce que fait la liste des fichiers. Quand un
choix mérite d'être retenu, il est écrit dans le commit : la raison vaut mieux que
le souvenir de la raison.

## Règles qui tiennent le site

- Les images se **déposent**, elles ne se collent pas en URL.
- Un chapô est déduit du début d'un texte, jamais saisi à côté.
- Une ressource est un lien, et ce lien est obligatoire.
- Un projet créé est publié d'office.
- Un compte se désactive, il ne se supprime pas : ses contributions gardent un auteur.
- Une sanction de modération reste réversible tant qu'elle n'est pas définitive.