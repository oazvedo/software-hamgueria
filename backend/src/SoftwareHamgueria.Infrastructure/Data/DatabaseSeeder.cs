using SoftwareHamgueria.Domain.Entities;

namespace SoftwareHamgueria.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static void Seed(AppDbContext context)
    {
        if (context.Dishes.Any()) return;

        var dishes = new[]
        {
            // Estagiários (Starters)
            new Dish("Bolinho de Estagiário", "Pequeno, barato e vem em grande quantidade. Serve pra qualquer coisa.", 12.90, Category.Starters),
            new Dish("Isca de Júnior Frita", "Crocante por fora, ainda aprendendo por dentro.", 16.50, Category.Starters),
            new Dish("Bruschetta Hello World", "O primeiro prato que todo estagiário aprende a montar.", 18.00, Category.Starters),

            // Monolitos & Microserviços (Hamburgers)
            new Dish("Burger Monolito", "Pilha generosa de carne. Difícil de escalar, mas resolve tudo sozinho.", 34.90, Category.Hamburgers),
            new Dish("Smash Microserviços", "Vários burgers pequenos que se comunicam muito bem entre si.", 39.90, Category.Hamburgers),
            new Dish("Combo Serverless", "Aparece só quando você chama e some depois. Você paga por mordida.", 32.00, Category.Hamburgers),
            new Dish("X-Tudo Legado", "Ninguém sabe direito como funciona, mas está em produção há 10 anos. Não mexa.", 42.00, Category.Hamburgers),

            // Clean Code / Refactor (Salads)
            new Dish("Salada Clean Code", "Sem gambiarra, bem indentada e fácil de manter.", 26.90, Category.Salads),
            new Dish("Bowl Refactor", "Os mesmos ingredientes de ontem, reorganizados até ficar elegante.", 28.50, Category.Salads),
            new Dish("Wrap DRY", "Don't Repeat Yourself: cada ingrediente aparece uma vez só.", 24.90, Category.Salads),

            // Chopp por Cargo & Combustível (Drinks)
            new Dish("Chopp Estagiário", "150ml. Copo americano, que é o que cabe no orçamento.", 6.00, Category.Drinks),
            new Dish("Chopp Júnior", "300ml. Já dá pra comemorar o primeiro deploy sem quebrar nada.", 10.00, Category.Drinks),
            new Dish("Chopp Pleno", "500ml. Pint cheia: você já sabe o que está fazendo.", 14.00, Category.Drinks),
            new Dish("Chopp Sênior", "700ml. Caneca respeitável pra quem revisa o PR dos outros.", 18.00, Category.Drinks),
            new Dish("Chopp Tech Lead", "1 litro. Metade é pra aguentar as reuniões de alinhamento.", 26.00, Category.Drinks),
            new Dish("Chopp Arquiteto", "Torre de 3 litros. Os diagramas ficam mais abstratos a cada gole.", 70.00, Category.Drinks),
            new Dish("Black Coffee (sem açúcar)", "Amargo como a alma do dev na sexta às 18h.", 8.00, Category.Drinks),
            new Dish("Energético Deploy Sexta", "500ml de coragem pra subir em produção antes do fim de semana.", 14.00, Category.Drinks),

            // Deploy em Produção (Dessert)
            new Dish("Brownie Merge Conflict", "Duas massas que não queriam se juntar, resolvidas no capricho.", 19.90, Category.Dessert),
            new Dish("Pudim CI/CD", "Passou em todos os testes e foi entregue liso, sem furos.", 17.50, Category.Dessert),
        };

        context.Dishes.AddRange(dishes);

        if (!context.Tables.Any())
        {
            var tables = Enumerable.Range(1, 8).Select(i => new Table($"Mesa {i:D2}")).ToArray();
            context.Tables.AddRange(tables);
        }

        context.SaveChanges();
    }
}
