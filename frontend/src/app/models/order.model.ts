/**
 * Represents a single line item within an order.
 * Corresponds to the OrderItem entity in the backend.
 */
export interface OrderItem {
    skuId: number;
    skuCode: string;
    quantity: number;
    unitPrice: number;
    subTotal: number;
}

/**
 * Represents a customer order.
 * Can be in states: 'Draft', 'Reserved', 'Confirmed', 'Cancelled'.
 */
export interface Order {
    id: number;
    storeId: number;
    storeName: string;
    status: string;
    totalAmount: number;
    createdAt: Date;
    items: OrderItem[];
}

/** DTO for submitting a single item in a new order. */
export interface CreateOrderItem {
    skuId: number;
    quantity: number;
}

/** DTO for creating a new order transaction. */
export interface CreateOrderRequest {
    storeId: number;
    items: CreateOrderItem[];
}

/**
 * Represents the current stock status of a SKU at a specific Store.
 * Used for the Order Creation UI.
 */
export interface InventoryItem {
    skuId: number;
    skuCode: string;
    productName: string;
    onHand: number;
    reserved: number;
    /** Helper property: Physical Stock - Reserved Stock */
    available: number;
}
