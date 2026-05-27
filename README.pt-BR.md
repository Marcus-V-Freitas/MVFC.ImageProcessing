# 📸 MVFC.ImageProcessing — Media Pipeline

[![Coverage](https://codecov.io/gh/Marcus-V-Freitas/MVFC.ImageProcessing/branch/main/graph/badge.svg)](https://codecov.io/gh/Marcus-V-Freitas/MVFC.ImageProcessing)
[![License](https://img.shields.io/badge/license-Apache--2.0-blue)](LICENSE)

> 🇺🇸 [Read in English](README.md)

Pipeline event-driven de processamento de imagens com normalização automática de formato, geração de thumbnails, captioning por IA e gerenciamento completo do ciclo de vida — 100% local, totalmente offline.

---

## 🎯 Motivação

Faça upload de qualquer imagem nos formatos suportados pelo Magick.NET (JPEG, PNG, AVIF, HEIC, TIFF, WebP, BMP e mais de 200 outros) e tenha automaticamente:

1. O arquivo **normalizado** para um formato web-safe (PNG)
2. Uma **miniatura** gerada para visualização rápida (200×200 PNG)
3. Uma **descrição em linguagem natural** gerada por IA (BLIP)
4. A possibilidade de **excluir** todos os artefatos com um clique

Tudo roda **localmente** na sua máquina, sem depender de nenhum serviço em nuvem pago. Os serviços do Google Cloud (Pub/Sub e Cloud Storage) são emulados via Docker, e a infraestrutura é provisionada automaticamente via Terraform.

---

## 📋 Pré-requisitos

| Ferramenta | Versão | Propósito |
|---|---|---|
| [Docker Desktop](https://www.docker.com/products/docker-desktop/) | 24+ | Runtime de containers |
| [Terraform](https://developer.hashicorp.com/terraform/downloads) | 1.5+ | Provisionamento de infraestrutura |
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0+ | Build e execução dos serviços C# (opcional para dev) |
| [Git](https://git-scm.com/) | 2.x | Controle de versão |
| `curl` | — | Health checks no script de start |

> **Nota:** Você **não** precisa ter Python, PyTorch ou qualquer biblioteca de ML instalada localmente. A Vision API roda inteiramente dentro do seu container Docker.

---

## 🚀 Como Rodar

```bash
# Clone o repositório
git clone https://github.com/Marcus-V-Freitas/MVFC.ImageProcessing.git
cd MVFC.ImageProcessing

# Subir todos os containers + provisionar infraestrutura
./scripts/start.sh

# Parar tudo e limpar
./scripts/stop.sh
```

O script `start.sh` executa os seguintes passos na ordem:

1. Verifica a infraestrutura existente (use `./scripts/start.sh --clean` para recriar do zero)
2. Builda e atualiza apenas os serviços modificados via `docker compose up -d --build`
3. Aguarda os health checks do PubSub, GCS e Vision API
4. Executa `terraform init && terraform apply` para garantir os tópicos, subscriptions e buckets

Após subir, acesse o **Dashboard** em [http://localhost:3000](http://localhost:3000).

### Endpoints Disponíveis

| Serviço | URL |
|---|---|
| Dashboard | http://localhost:3000 |
| Upload API | http://localhost:8081/upload |
| Vision API | http://localhost:5000/health |
| GCS Buckets | http://localhost:4443/storage/v1/b |
| PubSub Emulator | http://localhost:8681 |

---

## 🏗️ Visão Geral da Arquitetura

O pipeline segue uma arquitetura **event-driven com microserviços** usando **GCS Object Notifications**. Cada etapa do processamento é um serviço independente. Quando um worker grava um arquivo em um bucket, o Cloud Storage emite automaticamente uma notificação `OBJECT_FINALIZE` para um tópico Pub/Sub, que dispara o próximo estágio — **workers nunca publicam eventos explicitamente**.

Os arquivos são armazenados no **Google Cloud Storage** (emulado via `fake-gcs-server`) e os eventos fluem pelo **Google Cloud Pub/Sub** (emulado).

> **⚠️ Emulador vs Produção:** Em produção (GCP), o Cloud Storage envia nativamente notificações `OBJECT_FINALIZE` para tópicos Pub/Sub via [`google_storage_notification`](https://cloud.google.com/storage/docs/pubsub-notifications). O emulador do PubSub **não** suporta essa funcionalidade, então um **GCS Router** leve (`scripts/gcs_router.py`) faz polling de um tópico genérico `gcs-object-events` e roteia as mensagens para o tópico correto de cada bucket. Esse router existe **apenas no ambiente local/emulação** e não é necessário em produção.

```mermaid
graph LR
    U["👤 Usuário"] -->|Drag & Drop| DASH["Dashboard :3000"]
    U -->|"🗑️ Excluir"| DASH
    DASH -->|POST /upload| API["mvfc-image-upload-api :8081"]
    API -->|Salva imagem original| GCS[("Cloud Storage")]
    GCS -->|"OBJECT_FINALIZE"| PS{{"PubSub"}}
    PS -->|Push| CONV["mvfc-image-converter-worker :8084"]
    CONV -->|"Download + Normaliza PNG"| GCS
    GCS -->|"OBJECT_FINALIZE"| PS
    PS -->|Push| TW["mvfc-image-thumbnail-worker :8082"]
    PS -->|Push| IA["mvfc-image-analysis-worker :8083"]
    TW -->|"Download + Gera miniatura"| GCS
    IA -->|"Download + Base64"| VA["mvfc-image-vision-api :5000"]
    VA -->|BLIP captioning| VA
    IA -->|Salva analysis.json| GCS
    GCS -->|"OBJECT_FINALIZE"| PS
    DASH -->|"Pub: file-delete-requested"| PS
    PS -->|Push| DEL["mvfc-image-delete-worker :8086"]
    DEL -->|"Apaga dos 4 buckets"| GCS
    PS -->|SSE Push /pubsub/notify| DASH

    CONV -..->|"DLQ (após 5 falhas)"| DLQ[("Dead-Letter Topic")]
    TW -..->|"DLQ (após 5 falhas)"| DLQ
    IA -..->|"DLQ (após 5 falhas)"| DLQ
    DEL -..->|"DLQ (após 5 falhas)"| DLQ
```

---

## 📦 Componentes

| Componente | Tecnologia | Porta | Responsabilidade |
|---|---|---|---|
| **mvfc-image-upload-api** | .NET 10 Minimal API | `:8081` | Recebe uploads, salva no GCS (dispara pipeline via notificação) |
| **mvfc-image-converter-worker** | .NET 10 + Magick.NET | `:8084` | Normaliza qualquer formato → PNG, salva no bucket `converted` |
| **mvfc-image-thumbnail-worker** | .NET 10 + Magick.NET | `:8082` | Gera miniatura 200×200 em PNG |
| **mvfc-image-analysis-worker** | .NET 10 + Refit | `:8083` | Envia imagem convertida para API de visão IA, salva JSON de análise |
| **mvfc-image-vision-api** | Python 3.12 + Flask + BLIP | `:5000` | Gera descrição em linguagem natural |
| **mvfc-image-delete-worker** | .NET 10 | `:8086` | Exclui imagem dos 4 buckets |
| **mvfc-image-dashboard-ui** | .NET 10 + HTML/JS | `:3000` | Interface visual com galeria e controles |
| **mvfc-gcs-router** | Python 3.10 (apenas emulador) | — | Roteia notificações GCS para tópicos Pub/Sub por bucket |
| **PubSub Emulator** | thekevjames/gcloud-pubsub-emulator | `:8681` | Barramento de eventos (emulado) |
| **Cloud Storage** | fake-gcs-server | `:4443` | Armazenamento de objetos (emulado, com suporte a notificações) |
| **Terraform** | HCL | — | Provisiona tópicos, subscriptions, buckets e notificações |

---

## 🔄 Fluxos Detalhados

### 1. Upload & Processamento Completo

Este é o fluxo principal. Quando o usuário faz upload de uma imagem, ela passa por **3 estágios de processamento**. Cada estágio é disparado automaticamente por uma **GCS Object Notification** (`OBJECT_FINALIZE`) — workers nunca publicam eventos; eles simplesmente gravam arquivos no bucket apropriado e a notificação dispara o próximo estágio.

> **Mudança importante:** Após a conversão, os estágios de **Thumbnail** e **Análise por IA** agora rodam **em paralelo** (ambos assinam `file-converted-topic`), reduzindo o tempo total de processamento.

```mermaid
sequenceDiagram
    actor U as 👤 Usuário
    participant D as Dashboard
    participant API as mvfc-image-upload-api
    participant GCS as Cloud Storage
    participant PS as PubSub
    participant CONV as mvfc-image-converter-worker
    participant TW as mvfc-image-thumbnail-worker
    participant IA as mvfc-image-analysis-worker
    participant VA as mvfc-image-vision-api

    U->>D: Drag & Drop de "foto.avif"
    D->>API: POST /upload (multipart)
    API->>GCS: Upload → uploads/{guid}-foto.avif
    API-->>D: 202 Accepted

    Note over GCS,PS: OBJECT_FINALIZE (bucket uploads)
    GCS->>PS: Notificação → "file-uploaded-topic"

    Note over PS,CONV: ① Normalização

    PS->>CONV: Push /pubsub/push
    CONV->>GCS: Download uploads/{guid}-foto.avif
    CONV->>CONV: MagickImage → Format = PNG
    CONV->>GCS: Upload → converted/{guid}-foto.avif (PNG)

    Note over GCS,PS: OBJECT_FINALIZE (bucket converted)
    GCS->>PS: Notificação → "file-converted-topic"

    Note over PS,TW: ② Thumbnail (paralelo)
    Note over PS,IA: ③ Análise por IA (paralelo)

    par Geração de thumbnail
        PS->>TW: Push /pubsub/push
        TW->>GCS: Download converted/{guid}-foto.avif (PNG)
        TW->>TW: MagickImage → Resize(200,200) + PNG
        TW->>GCS: Upload thumbnails/thumb-{guid}-foto.png
    and Análise por IA
        PS->>IA: Push /pubsub/push
        IA->>GCS: Download converted/{guid}-foto.avif
        IA->>VA: POST /analyze (base64)
        VA->>VA: BLIP image captioning (~3-5s)
        VA-->>IA: {"description": "...", "dominant_colors": [...]}
        IA->>GCS: Upload analysis-results/analysis-{guid}-foto.avif.json
    end

    Note over GCS,PS: Notificações OBJECT_FINALIZE para thumbnails & analysis-results

    Note over D: ④ Dashboard atualiza via SSE (tempo real)
    PS->>D: Push /pubsub/notify → evento gallery-updated
    D-->>U: Busca /api/files e re-renderiza a galeria
```

### 2. Exclusão de Imagem

O usuário pode excluir qualquer imagem diretamente pela interface. A exclusão apaga **todos os artefatos relacionados** dos 4 buckets de uma vez.

> **Nota:** A exclusão é o único fluxo que ainda usa publicação explícita no Pub/Sub (a partir do Dashboard), pois é uma ação iniciada pelo usuário e não um evento de escrita no GCS.

```mermaid
sequenceDiagram
    actor U as 👤 Usuário
    participant D as Dashboard
    participant PS as PubSub
    participant DW as mvfc-image-delete-worker
    participant GCS as Cloud Storage

    U->>D: Clica 🗑️ no card da imagem
    D->>D: confirm("Excluir foto.avif?")
    D->>PS: Pub "file-delete-requested-topic"
    D-->>U: Feedback visual

    PS->>DW: Push /pubsub/push
    
    par Exclusão paralela
        DW->>GCS: DELETE uploads/{fileName}
        DW->>GCS: DELETE converted/{fileName}
        DW->>GCS: DELETE thumbnails/thumb-{fileName}
        DW->>GCS: DELETE analysis-results/analysis-{fileName}.json
    end

    DW-->>PS: 200 OK (ack)

    Note over D: Evento SSE dispara atualização da galeria
```

### 3. Normalização de Formato (Detalhe)

O converter é o **primeiro estágio** do pipeline. Ele garante que, independentemente do formato original (AVIF, HEIC, TIFF, BMP...), todos os arquivos downstream sejam tratados como PNG. O arquivo convertido é salvo em um **bucket dedicado `converted`**, preservando o original em `uploads`.

```mermaid
sequenceDiagram
    participant PS as PubSub
    participant CONV as mvfc-image-converter-worker
    participant GCS as Cloud Storage

    PS->>CONV: Push (file-uploaded via notificação GCS)
    CONV->>GCS: Download do bucket uploads/
    
    alt Formato suportado (AVIF, HEIC, TIFF, WebP, BMP...)
        CONV->>CONV: new MagickImage(stream)
        CONV->>CONV: image.Format = MagickFormat.Png
        CONV->>GCS: Upload para bucket converted/ como PNG
        Note over GCS,PS: OBJECT_FINALIZE dispara file-converted-topic ✅
    else Arquivo corrompido ou inválido
        CONV->>CONV: catch(Exception)
        CONV->>CONV: Log de erro crítico
        CONV-->>PS: 500 Internal Server Error (Nack)
        Note over PS: Retenta até 5 vezes
        PS->>PS: Envia para dead-letter-topic
    end
```

> **Por que normalizar?** Navegadores não conseguem exibir formatos como TIFF, HEIC ou BMP nativamente. Ao converter tudo para PNG no início do pipeline, garantimos que a imagem exibida no Dashboard **sempre funcione** — sem ícones de imagem quebrada.

### 4. GCS Router (Apenas Emulador)

Em produção na GCP, recursos `google_storage_notification` enviam automaticamente eventos `OBJECT_FINALIZE` dos buckets para tópicos Pub/Sub. O emulador `fake-gcs-server` suporta enviar eventos para um **único tópico genérico** (`gcs-object-events`), mas não consegue rotear para tópicos diferentes por bucket.

O **GCS Router** (`scripts/gcs_router.py`) preenche essa lacuna:

```mermaid
sequenceDiagram
    participant GCS as fake-gcs-server
    participant GENERIC as tópico gcs-object-events
    participant ROUTER as mvfc-gcs-router
    participant TARGET as Tópico por Bucket

    GCS->>GENERIC: OBJECT_FINALIZE (qualquer bucket)
    ROUTER->>GENERIC: Pull de mensagens
    ROUTER->>ROUTER: Inspeciona payload.bucket
    
    alt bucket = "uploads"
        ROUTER->>TARGET: Publish → file-uploaded-topic
    else bucket = "converted"
        ROUTER->>TARGET: Publish → file-converted-topic
    else bucket = "thumbnails"
        ROUTER->>TARGET: Publish → thumbnail-created-topic
    else bucket = "analysis-results"
        ROUTER->>TARGET: Publish → analysis-completed-topic
    end
```

> **Este componente não existe em produção.** Na GCP, cada recurso `google_storage_notification` envia eventos diretamente para o tópico correto. A configuração Terraform inclui esses recursos mas os pula localmente via `count = var.is_local ? 0 : 1`.

---

## 🧩 Topologia de Eventos (Pub/Sub)

Cada seta representa um tópico Pub/Sub com sua respectiva subscription push. **Os eventos são produzidos por GCS Object Notifications** (não pelos workers), exceto pelo fluxo de exclusão que é iniciado pelo usuário.

```mermaid
graph TD
    GCS[("Cloud Storage")] -->|"OBJECT_FINALIZE"| T1["file-uploaded-topic"]
    T1 -->|mvfc-image-converter-worker-sub| CONV["mvfc-image-converter-worker"]
    GCS -->|"OBJECT_FINALIZE"| T2["file-converted-topic"]
    T2 -->|mvfc-image-thumbnail-worker-sub| TW["mvfc-image-thumbnail-worker"]
    T2 -->|mvfc-image-analysis-worker-sub| IA["mvfc-image-analysis-worker"]
    GCS -->|"OBJECT_FINALIZE"| T3["thumbnail-created-topic"]
    GCS -->|"OBJECT_FINALIZE"| T5["analysis-completed-topic"]
    T5 -->|mvfc-dashboard-analysis-sub| DASH["mvfc-image-dashboard-ui"]
    T4["file-delete-requested-topic"] -->|mvfc-image-delete-worker-sub| DEL["mvfc-image-delete-worker"]
    T1 -->|mvfc-dashboard-upload-sub| DASH
    T2 -->|mvfc-dashboard-convert-sub| DASH
    T3 -->|mvfc-dashboard-thumbnail-sub| DASH
    T4 -->|mvfc-dashboard-delete-sub| DASH

    CONV -..->|Max retentativas| DLQ["dead-letter-topic"]
    TW -..->|Max retentativas| DLQ
    IA -..->|Max retentativas| DLQ
    DEL -..->|Max retentativas| DLQ
```

| Tópico | Gatilho | Consumidor | Ack Deadline |
|---|---|---|---|
| `file-uploaded-topic` | Notificação GCS (bucket `uploads`) | mvfc-image-converter-worker, mvfc-image-dashboard-ui | 60s |
| `file-converted-topic` | Notificação GCS (bucket `converted`) | mvfc-image-thumbnail-worker, mvfc-image-analysis-worker, mvfc-image-dashboard-ui | 600s |
| `thumbnail-created-topic` | Notificação GCS (bucket `thumbnails`) | mvfc-image-dashboard-ui | 600s |
| `analysis-completed-topic` | Notificação GCS (bucket `analysis-results`) | mvfc-image-dashboard-ui | 10s |
| `file-delete-requested-topic` | mvfc-image-dashboard-ui (publicação explícita) | mvfc-image-delete-worker, mvfc-image-dashboard-ui | 30s |
| `gcs-object-events` | fake-gcs-server (apenas emulador) | mvfc-gcs-router | — |

---

## 🗄️ Buckets (Cloud Storage)

| Bucket | Conteúdo | Escrito por | Lido por | Notificação GCS → Tópico |
|---|---|---|---|---|
| `uploads` | Imagem original (qualquer formato) | mvfc-image-upload-api | mvfc-image-converter-worker | `file-uploaded-topic` |
| `converted` | Imagem normalizada em PNG | mvfc-image-converter-worker | mvfc-image-thumbnail-worker, mvfc-image-analysis-worker, mvfc-image-dashboard-ui | `file-converted-topic` |
| `thumbnails` | Miniaturas 200×200 em PNG | mvfc-image-thumbnail-worker | mvfc-image-dashboard-ui | `thumbnail-created-topic` |
| `analysis-results` | JSON com descrição gerada por IA e cores dominantes | mvfc-image-analysis-worker | mvfc-image-dashboard-ui | `analysis-completed-topic` |

---

## 🛠️ Tecnologias & Decisões

### Por que Magick.NET?

A biblioteca de processamento de imagens é essencial para dois workers: o converter (normalização) e o gerador de thumbnails.

| Critério | ~~SixLabors.ImageSharp~~ | **Magick.NET** ✅ |
|---|---|---|
| **Licença** | Paga (v4+) ou vulnerável (v3.x) | Apache 2.0 (gratuita) |
| **AVIF** | ❌ Não suporta | ✅ Nativo |
| **HEIC/HEIF** | ❌ | ✅ |
| **Formatos totais** | ~12 | **200+** |
| **Deps nativas no Docker** | Nenhuma | Embutidas no NuGet |

**Pacote usado:** `Magick.NET-Q8-AnyCPU` v14.13.1 (Q8 = 8 bits por canal — suficiente para web e mais leve em memória).

### Por que BLIP (Salesforce)?

Para gerar descrições das imagens em linguagem natural, usamos o modelo **BLIP** (Bootstrapping Language-Image Pre-training).

| Critério | Decisão |
|---|---|
| **Modelo** | `Salesforce/blip-image-captioning-base` |
| **Runtime** | PyTorch CPU-only |
| **Latência** | ~3-5 segundos por imagem |
| **Qualidade** | Descrições naturais e legíveis |
| **Offline** | ✅ Modelo pré-baixado durante o Docker build |

Alternativas descartadas:
- **YOLOv8** — Retornava tags genéricas e imprecisas ("person", "dining table")
- **Ollama (LLaVA)** — Muito lento em CPU (~30s), pesado para uso local

### Por que Refit para o Cliente da Vision API?

O `mvfc-image-analysis-worker` usa [Refit](https://github.com/reactiveui/refit) para chamar a Vision API em Python. Isso fornece um cliente HTTP tipado e declarativo via interface (`IVisionApiClient`), substituindo chamadas brutas de `HttpClient` e tornando o serviço mais fácil de testar e manter.

### Por que GCS Object Notifications?

Ao invés de cada worker publicar explicitamente no próximo tópico Pub/Sub, utilizamos **GCS Object Notifications** (`OBJECT_FINALIZE`). Isso significa:

- **Zero acoplamento entre estágios**: Workers só precisam saber *em qual bucket gravar*. Não precisam de clientes Pub/Sub, nomes de tópicos ou lógica de publicação.
- **Handlers mais simples**: Os handlers do domínio não dependem mais de `IPublishService` — apenas armazenam arquivos e retornam.
- **Emissão automática de eventos**: Gravar um arquivo em um bucket é suficiente para disparar o próximo estágio. A integração GCS → Pub/Sub é gerenciada no nível de infraestrutura (Terraform).
- **Push subscriptions**: Cada worker é uma Minimal API que expõe um endpoint `/pubsub/push`. O Pub/Sub entrega as mensagens automaticamente.
- **Retry automático**: Se um worker estiver indisponível, o Pub/Sub reentrega a mensagem após o `ack_deadline_seconds`.

> **Nota sobre emulador:** Como o `fake-gcs-server` não consegue rotear para tópicos por bucket, o sidecar `mvfc-gcs-router` faz esse roteamento localmente. Em produção, os recursos `google_storage_notification` do Terraform fazem isso nativamente.

### Por que emuladores locais?

| Serviço | Emulador | Motivo |
|---|---|---|
| Pub/Sub | `gcloud beta emulators pubsub` | Zero custo, funciona offline |
| Cloud Storage | `fake-gcs-server` | API compatível com GCS real |
| Terraform | Provider Google | Provisiona contra os emuladores |

**Vantagem**: O código dos workers é **idêntico** ao que rodaria na GCP real. A única diferença é a variável de ambiente `*_EMULATOR_HOST`.

---

## 🧪 Testes

O projeto inclui um projeto de testes preparado para testes unitários e de integração:

```bash
dotnet test
```

Você também pode usar o arquivo HTTP em `scripts/mvfc.image-processing.http` para testes manuais da API (compatível com VS Code REST Client / JetBrains HTTP Client).

---

## 📁 Estrutura do Projeto

```
MVFC.ImageProcessing/
├── src/
│   ├── MVFC.Image.Domain/                 # Regras de negócio, Contratos e CQRS Handlers
│   ├── MVFC.Image.Infra/                  # Implementações GCP (Storage e Pub/Sub)
│   ├── MVFC.Image.IoC/                    # Injeção de Dependências e Configurações
│   ├── MVFC.Image.Shareable/              # Eventos, DTOs compartilhados e mapper de notificações GCS
│   ├── MVFC.ImageUpload.Api/              # Recebe uploads via HTTP
│   ├── MVFC.ImageConverter.Worker/        # Normaliza qualquer formato → PNG (salva no bucket converted)
│   ├── MVFC.ImageThumbnail.Worker/        # Gera miniaturas 200×200
│   ├── MVFC.ImageAnalysis.Worker/         # Orquestra análise por IA (Refit + Polly)
│   ├── MVFC.ImageVision.Api/              # Modelo BLIP (Python/Flask)
│   ├── MVFC.ImageDelete.Worker/           # Exclui arquivos dos 4 buckets
│   └── MVFC.ImageDashboard.UI/            # Interface web (HTML/JS)
├── tests/
│   ├── MVFC.Image.Domain.Tests/           # Testes unitários do Domain
│   ├── MVFC.Image.Infra.Tests/            # Testes unitários de Infra
│   ├── MVFC.Image.Shareable.Tests/        # Testes unitários do Shareable (incl. GcsNotificationMapper)
│   ├── MVFC.ImageUpload.Api.Tests/        # Testes de integração da API de Upload
│   ├── MVFC.ImageConverter.Worker.Tests/  # Testes de integração do Converter
│   ├── MVFC.ImageThumbnail.Worker.Tests/  # Testes de integração do Thumbnail
│   ├── MVFC.ImageAnalysis.Worker.Tests/   # Testes de integração do Analysis (IA)
│   ├── MVFC.ImageDelete.Worker.Tests/     # Testes de integração do Delete
│   └── MVFC.ImageDashboard.UI.Tests/      # Testes de integração do Dashboard UI
├── scripts/
│   ├── start.sh                           # Sobe toda a infraestrutura
│   ├── stop.sh                            # Derruba tudo
│   ├── gcs_router.py                      # Roteador de notificações GCS (apenas emulador)
│   └── mvfc.image-processing.http         # Amostras de requisições HTTP
├── terraform/                             # IaC: tópicos, subs, buckets, notificações
├── samples/                               # Imagens de exemplo para testes
├── docker-compose.yml                     # Orquestração dos containers
├── MVFC.ImageProcessing.slnx              # Arquivo de solução
├── Directory.Build.props                  # Propriedades MSBuild compartilhadas
├── Directory.Build.targets                # Targets MSBuild (analyzers)
├── Directory.Packages.props               # Gerenciamento central de pacotes
├── CONTRIBUTING.md                        # Guia de contribuição
├── SECURITY.md                            # Política de segurança
├── LICENSE                                # Apache 2.0
├── README.md                              # Versão em inglês
└── README.pt-BR.md                        # ← Você está aqui! (Português)
```

---

## ⚙️ Padrões Avançados de Arquitetura

Este projeto implementa padrões de sistemas distribuídos de nível corporativo:
- **GCS Object Notifications:** Workers não publicam eventos — eles gravam arquivos nos buckets, e o GCS emite automaticamente notificações `OBJECT_FINALIZE` para o tópico Pub/Sub correspondente. Isso elimina `IPublishService` de todos os handlers (exceto o fluxo de exclusão do Dashboard) e reduz o acoplamento.
- **GCS Notification Router (Emulador):** Como o emulador `fake-gcs-server` só publica em um único tópico genérico, um sidecar leve em Python (`scripts/gcs_router.py`) faz polling de `gcs-object-events` e republica no tópico correto por bucket. Este componente é implantado condicionalmente e não tem equivalente em produção.
- **Dead-Letter Queues (DLQ):** Configurado via Terraform. Se um worker falhar ao processar uma mensagem (ex: um arquivo corrompido) 5 vezes, ela é redirecionada de forma segura para o `dead-letter-topic` em vez de causar retentativas infinitas.
- **Circuit Breakers & Retries:** As chamadas HTTP para a Vision API são encapsuladas com `Microsoft.Extensions.Http.Resilience`, garantindo retentativas automáticas, timeouts e circuit breakers contra falhas transientes do modelo de IA.
- **Processamento Paralelo:** Após a conversão, os workers de Thumbnail e Analysis assinam ambos o `file-converted-topic` e rodam concorrentemente, reduzindo a latência total do pipeline.

---

## 🔒 Privacidade e Segurança

Um princípio central deste projeto é a **Privacidade de Dados**. Como todo o pipeline (incluindo o modelo de IA) roda localmente via Docker:
- Suas imagens **nunca** saem da sua máquina.
- Nenhuma chave de API de terceiros é necessária.
- Sem custos de armazenamento na nuvem ou mineração de dados.
- Adequado para processamento de mídias sensíveis, pessoais ou confidenciais.

---

## 🚑 Solução de Problemas

- **Portas em uso:** Se portas como `:3000`, `:5000` ou `:8081` estiverem ocupadas, os containers não iniciarão. Pare os serviços conflitantes ou mapeie portas diferentes no `docker-compose.yml`.
- **A primeira execução é demorada:** Na primeira vez que você rodar `./scripts/start.sh`, o Docker baixará o modelo Salesforce BLIP (~1,5 GB). As inicializações subsequentes serão imediatas.
- **Imagens não aparecem no Dashboard:** Verifique se o emulador Pub/Sub e o provisionamento do Terraform foram concluídos com sucesso. Você pode ver os logs dos workers via `docker compose logs -f`.
- **Thumbnails não carregam:** O nome do thumbnail sempre usa a extensão `.png` independente do formato original (ex: `thumb-{guid}-foto.png`). Verifique se o Dashboard está buscando pelo nome correto.
- **Upload rejeitado com 400:** O validador aceita qualquer `image/*`. Certifique-se de que o arquivo é uma imagem válida e que o nome não contém caracteres reservados pelo OS (`\`, `/`, `:`, `*`, `?`, `"`, `<`, `>`, `|`).

---

## Contribuição

Consulte [CONTRIBUTING.md](CONTRIBUTING.md).

---

## 📄 Licença

Este projeto está licenciado sob a [Licença Apache 2.0](LICENSE).
