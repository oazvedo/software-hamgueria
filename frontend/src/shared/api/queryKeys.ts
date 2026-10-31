export const queryKeys = {
  dishes: ['dishes'] as const,
  tables: ['tables'] as const,
  order: (id: string) => ['order', id] as const,
  comanda: (tableId: string, customer: string) => ['comanda', tableId, customer] as const,
  tableOrders: (tableId: string) => ['tableOrders', tableId] as const,
  tableComandas: (tableId: string) => ['tableComandas', tableId] as const,
  allOrders: ['allOrders'] as const,
  audit: (table: string) => ['audit', table] as const,
}
