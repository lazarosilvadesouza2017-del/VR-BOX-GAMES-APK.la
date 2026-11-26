# SpiderVR (VR Box)

Base: Spider-Man Swinging Simulator (Unity 2021.3.7f1) + cidade/trafego do Unity2023-Traffic + traje GLB/FBX. `_reference/` = TPS combat + docs SpidyVR (nao compilados).

## Gestos
- Teia: indicador + mindinho (esq = balancar, dir = puxar). Arminha (indicador + polegar) = pegar arma / atirar.
- Soco/chute reais -> soco/chute no jogo. Puxao (mao desce rapido) = puxa a teia (modo "Puxar") ou joga preso no chao.
- Duas teias + juntar as maos: coisa+coisa = arremesso; coisa+chao = corda; estrutura+estrutura = barreira de teia.
- Pulo: pulinho / normal / alto (forca do impulso); correndo + pulo = pulo com impulso. Agachar: joelho dobrado ou quadril descendo.
- Sem MediaPipe: toque na tela (1 dedo = teia esq, 2 = dir, 3 = juntar, deslizar p/ baixo = puxao). Correr = tremer o celular.

## Trajes (escolha ao entrar no mapa, solo ou multiplayer)
14 trajes com nome e foto (retrato 3D gerado na hora): 13 com esqueleto (Spider-Man Unlimited / TASM2) + Brand New Day (estatico).
Os modelos vieram SEM animacao: bracos/pernas balancam por codigo (SuitAnimator). Poderes unicos por traje = proxima etapa.

## Maos/corpo pela camera (MediaPipe Tasks)
Modelos em `Assets/StreamingAssets` (hand_landmarker.task, pose_landmarker_lite.task). A biblioteca e baixada do Maven Central no build do GitHub
(`Editor/MPGradlePatch.cs`) e usada por `Plugins/Android/MPBridge.java`. Opcoes > "Rastrear maos/corpo pela camera" (ligado por padrao).

## Limitacoes reais (nada disso foi compilado: sem Unity aqui)
1. Se o build do GitHub falhar, o erro mais provavel esta na parte do MediaPipe/Gradle ou em algum script: mande o log vermelho.
2. Esqueleto sem clipes de animacao; direcao frontal dos modelos .dae nao verificada.
3. Online precisa de projeto Unity Gaming Services (Relay + Lobby) vinculado.
4. Distorcao de lente da VR Box nao implementada. Barreiras e bandidos nao sao sincronizados entre jogadores (cada um ve os seus).
5. Spider-Man_Rigged_3d_Model.rar nao foi aberto (sem ferramenta de RAR): mande em .zip.
