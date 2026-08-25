# Detalhamento técnico
Este detalhamento complementa os requisitos solicitados no documento do desafio técnico.

Vídeo de apresentação: https://www.youtube.com/watch?v=QNIF8KjEMqA

1. Arquitetura
- Frontend desenvolvido em Angular.
- Inventory API, responsável por produtos e movimentações de estoque.
- Billing API, responsável pela criação, consulta e impressão das notas fiscais.

Cada microsserviço possui seu próprio contexto de dados e banco lógico:

```text
Angular
├── Inventory API ──> InventoryDb
└── Billing API   ──> BillingDb
        └───────────> Inventory API
```

Durante a criação da nota, o Billing valida no Inventory se os produtos existem. Na impressão, o Billing solicita a baixa do estoque e somente altera a nota para Fechada depois que o Inventory confirma a operação.

## 2. Ciclos de vida do Angular

O ciclo de vida utilizado no frontend foi o `OnInit`.

Ele está presente nos componentes que precisam carregar dados assim que são inicializados, incluindo:

- Listagem de produtos.
- Gestão de estoque.
- Listagem de notas fiscais.
- Formulário de criação da nota.
- Detalhes da nota fiscal.

Na listagem de produtos, o `ngOnInit` inicia a consulta dos produtos. O mesmo padrão é utilizado para buscar notas e saldos. Na criação da nota, ele carrega os produtos disponíveis para preencher o dropdown. Nos detalhes, inicia a consulta da nota e das informações dos produtos.

Não foi necessário utilizar `OnDestroy` porque as chamadas realizadas pelo `HttpClient` emitem uma resposta e são finalizadas automaticamente. Os Observables retornados pelo fechamento das dialogs também completam após a modal ser encerrada.

Caso futuramente sejam adicionados Observables contínuos, como WebSockets, intervalos ou eventos globais, seria recomendável utilizar `takeUntilDestroyed`, disponibilizado pelo Angular, para evitar subscriptions ativas depois da destruição do componente.

## 3. RxJS

O frontend utiliza RxJS para controlar as operações assíncronas realizadas pelos serviços Angular.

### Observable

Os métodos dos serviços retornam Observables tipados:

```typescript
getAll(): Observable<Product[]> {
  return this.http.get<Product[]>(this.productsUrl);
}
```

Isso permite que o componente trate separadamente sucesso, erro e finalização.

### `finalize`

O operador `finalize` restaura os indicadores de carregamento independentemente do resultado da requisição:

```typescript
this.productService
  .getAll()
  .pipe(
    finalize(() => {
      this.loading = false;
    }),
  )
  .subscribe({
    next: (products) => {
      this.products = products;
    },
    error: () => {
      this.errorMessage = 'Não foi possível carregar os produtos.';
    },
  });
```

Ele também é utilizado nos estados de envio e impressão, garantindo que botões e indicadores de processamento retornem ao estado correto após sucesso ou erro.

### `forkJoin`

Nos detalhes da nota, `forkJoin` executa em paralelo:

- Consulta da nota fiscal.
- Consulta dos produtos.

O componente aguarda as duas operações terminarem antes de montar a tela.

### `catchError` e `of`

Ao carregar os detalhes, uma falha na consulta auxiliar dos produtos é tratada com `catchError`. O operador `of` fornece uma coleção vazia como fallback, permitindo que os demais dados da nota ainda sejam exibidos.

### `afterClosed`

O Observable retornado por `MatDialog.afterClosed()` informa se uma modal criou ou modificou dados. Quando isso acontece, a listagem correspondente é recarregada.

## 4. Formulários Angular

Os formulários foram desenvolvidos com Reactive Forms.

No cadastro de produtos, os campos possuem validadores como:

- `required`.
- `min`.
- `maxLength`.

Na criação de notas foi utilizado `FormArray`, permitindo adicionar e remover dinamicamente várias linhas de produtos:

```typescript
items: this.formBuilder.array([
  this.createItemForm(),
]);
```

Cada elemento do `FormArray` possui o identificador do produto e sua quantidade. Antes do envio, o formulário é validado e convertido no formato esperado pela Billing API.

## 5. Componentes visuais

A interface utiliza Angular Material e Angular CDK.

Foram utilizados:

- `MatDialog` para formulários, detalhes e confirmações.
- `MatTable` para listagens.
- `MatFormField` e `MatInput` nos formulários.
- `MatSelect` para seleção dos produtos.
- `MatSnackBar` para mensagens de sucesso e erro.
- `MatProgressSpinner` para indicadores de processamento.
- Botões e ícones do Angular Material.

O frontend foi estruturado com componentes standalone e rotas com lazy loading, reduzindo o carregamento inicial e separando as funcionalidades por domínio.

## 6. Backend em C# e .NET

Os microsserviços foram desenvolvidos em C# com:

- .NET 10.
- ASP.NET Core Web API.
- Entity Framework Core 10.
- Provider do Entity Framework para SQL Server.
- Swagger/OpenAPI para documentação dos endpoints.
- Injeção de dependência nativa do ASP.NET Core.
- `HttpClient` tipado para comunicação entre Billing e Inventory.

A organização interna separa:

- Controllers.
- Services.
- DTOs.
- Models.
- Mappings.
- Exceptions.
- Persistência com `DbContext`.

## 7. LINQ

O projeto utiliza LINQ tanto para manipulação de objetos em memória quanto para consultas executadas pelo Entity Framework Core.

### LINQ to Objects

Na criação das notas e nas movimentações de estoque, os itens recebidos são agrupados pelo produto:

```csharp
var groupedItems = request.Items
    .GroupBy(item => item.ProductId)
    .Select(group => new
    {
        ProductId = group.Key,
        Quantity = group.Sum(item => item.Quantity)
    })
    .ToList();
```

Esse agrupamento evita que o mesmo produto seja processado separadamente quando aparecer mais de uma vez na requisição. As quantidades são somadas antes da validação e persistência.

Também são utilizados:

- `Select` para transformar entidades em DTOs.
- `Where` para filtrar dados.
- `First` para localizar produtos já carregados.
- `ToHashSet` para facilitar a identificação de produtos inexistentes.
- `Sum` para consolidar quantidades.

### LINQ to Entities

Nas consultas realizadas pelo Entity Framework, as expressões LINQ são traduzidas para SQL.

- `AnyAsync` para verificar se um código de produto já existe.
- `FirstOrDefaultAsync` para localizar produto ou nota pelo identificador.
- `Where` e `Contains` para consultar vários produtos.
- `Include` para carregar os itens de uma nota.
- `OrderByDescending` para ordenar notas pela numeração.
- `ToListAsync` para executar consultas de maneira assíncrona.
- `AsNoTracking` em consultas somente para leitura.

Exemplo:

```csharp
var invoices = await context.Invoices
    .AsNoTracking()
    .Include(invoice => invoice.Items)
    .OrderByDescending(invoice => invoice.Number)
    .ToListAsync(cancellationToken);
```

O `AsNoTracking` reduz o custo de rastreamento do Entity Framework em consultas que não modificarão as entidades.

## 8. SQL Server

O sistema utiliza Microsoft SQL Server 2022 executado em Docker, com um volume persistente.

Foi decidido usar SQL Server por fazer parte do ecossistema Microsoft e possuir integração direta com a plataforma .NET por meio do Entity Framework Core e do provider `Microsoft.EntityFrameworkCore.SqlServer`. Também por conter algumas coisas específicas pra tratar concorrência/idempotência que poderiam ser aplicadas.

A persistência é organizada em dois bancos lógicos:

- `InventoryDb`: produtos e saldos.
- `BillingDb`: notas fiscais e itens.

O mapeamento é realizado pelo Entity Framework Core usando a abordagem Code First e migrations.

Entre as restrições utilizadas estão:

- Chaves primárias em GUID.
- Índice único para o código do produto.
- Numeração da nota com coluna identity.
- Índice único para a numeração da nota.
- Relacionamento entre nota e itens.
- Exclusão em cascata dos itens quando uma nota é removida.
- Status da nota persistido como texto.

O volume do Docker garante que os dados continuem disponíveis depois que o contêiner é reiniciado.

## 9. Tratamento de erros e exceções

Cada microsserviço possui um manipulador global baseado em `IExceptionHandler`.

As exceções de domínio são transformadas em respostas `ProblemDetails`, contendo status HTTP, título e descrição.

Entre os cenários tratados estão:

- Produto inexistente: HTTP 404 ou 400, dependendo da operação.
- Código duplicado: HTTP 409.
- Estoque insuficiente: HTTP 409.
- Nota inexistente: HTTP 404.
- Tentativa de imprimir nota fechada: HTTP 409.
- Inventory indisponível: HTTP 503.
- Erro inesperado: HTTP 500.

No frontend, o campo `detail` da resposta é apresentado ao usuário por mensagens ou estados de erro. A tela também oferece nova tentativa quando uma consulta não pode ser concluída.

## 10. Considerações finais

O ideal, pra tratar concorrência e idempotência, seria:

- Atualização atômica ou `rowversion`.
- Transação local no Inventory.
- Chave de idempotência com índice único.
- Propagação do identificador entre os serviços.
- Outbox e Inbox para maior confiabilidade distribuída.

