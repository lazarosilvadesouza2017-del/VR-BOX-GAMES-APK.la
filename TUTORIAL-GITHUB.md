# Compilar o APK no GitHub (passo a passo)

1. Crie um repositorio novo no GitHub (privado serve) e suba TODO o conteudo deste zip na branch `main`
   (o `.github/workflows/build-apk.yml` precisa ficar na raiz).
   Arquivos grandes: o GitHub bloqueia acima de 100 MB; os arquivos daqui estao abaixo disso.
2. Licenca Unity (gratuita, Personal): siga https://game.ci/docs/github/activation
   e crie em Settings > Secrets and variables > Actions:
   `UNITY_LICENSE`, `UNITY_EMAIL`, `UNITY_PASSWORD`.
3. Rode: aba Actions > "Build APK" > Run workflow. Ou crie a tag (gera Release com o APK):

       git tag v0.2.0
       git push origin v0.2.0

4. Baixe `SpiderVR.apk` em Actions > (execucao) > Artifacts > SpiderVR-apk, ou em Releases.
   Instale no celular (permita "fontes desconhecidas").

Se o build falhar, abra a execucao em Actions e me mande o erro em vermelho.
