import type { Dish, Table } from '@/shared/api/contracts'

const now = new Date().toISOString()

export const mockDishes: Dish[] = [
  // Estagiários (Starters)
  { id: 'd1', name: 'Bolinho de Estagiário', description: 'Pequeno, barato e vem em grande quantidade. Serve pra qualquer coisa.', price: 12.9, category: 'Starters', isActive: true, createdAt: now, updatedAt: now },
  { id: 'd2', name: 'Isca de Júnior Frita', description: 'Crocante por fora, ainda aprendendo por dentro.', price: 16.5, category: 'Starters', isActive: true, createdAt: now, updatedAt: now },
  { id: 'd3', name: 'Bruschetta Hello World', description: 'O primeiro prato que todo estagiário aprende a montar.', price: 18.0, category: 'Starters', isActive: true, createdAt: now, updatedAt: now },

  // Monolitos & Microserviços (Hamburgers)
  { id: 'd4', name: 'Burger Monolito', description: 'Pilha generosa de carne. Difícil de escalar, mas resolve tudo sozinho.', price: 34.9, category: 'Hamburgers', isActive: true, createdAt: now, updatedAt: now },
  { id: 'd5', name: 'Smash Microserviços', description: 'Vários burgers pequenos que se comunicam muito bem entre si.', price: 39.9, category: 'Hamburgers', isActive: true, createdAt: now, updatedAt: now },
  { id: 'd6', name: 'Combo Serverless', description: 'Aparece só quando você chama e some depois. Você paga por mordida.', price: 32.0, category: 'Hamburgers', isActive: true, createdAt: now, updatedAt: now },
  { id: 'd7', name: 'X-Tudo Legado', description: 'Ninguém sabe direito como funciona, mas está em produção há 10 anos. Não mexa.', price: 42.0, category: 'Hamburgers', isActive: true, createdAt: now, updatedAt: now },

  // Clean Code / Refactor (Salads)
  { id: 'd8', name: 'Salada Clean Code', description: 'Sem gambiarra, bem indentada e fácil de manter.', price: 26.9, category: 'Salads', isActive: true, createdAt: now, updatedAt: now },
  { id: 'd9', name: 'Bowl Refactor', description: 'Os mesmos ingredientes de ontem, reorganizados até ficar elegante.', price: 28.5, category: 'Salads', isActive: true, createdAt: now, updatedAt: now },
  { id: 'd10', name: 'Wrap DRY', description: "Don't Repeat Yourself: cada ingrediente aparece uma vez só.", price: 24.9, category: 'Salads', isActive: true, createdAt: now, updatedAt: now },

  // Chopp por Cargo & Combustível (Drinks)
  { id: 'd11', name: 'Chopp Estagiário', description: '150ml. Copo americano, que é o que cabe no orçamento.', price: 6.0, category: 'Drinks', isActive: true, createdAt: now, updatedAt: now },
  { id: 'd12', name: 'Chopp Júnior', description: '300ml. Já dá pra comemorar o primeiro deploy sem quebrar nada.', price: 10.0, category: 'Drinks', isActive: true, createdAt: now, updatedAt: now },
  { id: 'd13', name: 'Chopp Pleno', description: '500ml. Pint cheia: você já sabe o que está fazendo.', price: 14.0, category: 'Drinks', isActive: true, createdAt: now, updatedAt: now },
  { id: 'd14', name: 'Chopp Sênior', description: '700ml. Caneca respeitável pra quem revisa o PR dos outros.', price: 18.0, category: 'Drinks', isActive: true, createdAt: now, updatedAt: now },
  { id: 'd15', name: 'Chopp Tech Lead', description: '1 litro. Metade é pra aguentar as reuniões de alinhamento.', price: 26.0, category: 'Drinks', isActive: true, createdAt: now, updatedAt: now },
  { id: 'd16', name: 'Chopp Arquiteto', description: 'Torre de 3 litros. Os diagramas ficam mais abstratos a cada gole.', price: 70.0, category: 'Drinks', isActive: true, createdAt: now, updatedAt: now },
  { id: 'd17', name: 'Black Coffee (sem açúcar)', description: 'Amargo como a alma do dev na sexta às 18h.', price: 8.0, category: 'Drinks', isActive: true, createdAt: now, updatedAt: now },
  { id: 'd18', name: 'Energético Deploy Sexta', description: '500ml de coragem pra subir em produção antes do fim de semana.', price: 14.0, category: 'Drinks', isActive: true, createdAt: now, updatedAt: now },

  // Deploy em Produção (Dessert)
  { id: 'd19', name: 'Brownie Merge Conflict', description: 'Duas massas que não queriam se juntar, resolvidas no capricho.', price: 19.9, category: 'Dessert', isActive: true, createdAt: now, updatedAt: now },
  { id: 'd20', name: 'Pudim CI/CD', description: 'Passou em todos os testes e foi entregue liso, sem furos.', price: 17.5, category: 'Dessert', isActive: true, createdAt: now, updatedAt: now },
]

export const mockTables: Table[] = Array.from({ length: 8 }, (_, i) => ({
  id: `t${i + 1}`,
  tableName: `Mesa ${String(i + 1).padStart(2, '0')}`,
  isActive: true,
  createdAt: now,
  updatedAt: now,
}))
