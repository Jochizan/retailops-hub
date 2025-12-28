export interface OrderItem {
    skuId: number;
    skuCode: string;
    quantity: number;
    unitPrice: number;
    subTotal: number;
}

export interface Order {
    id: number;
    storeId: number;
    storeName: string;
    status: string;
    totalAmount: number;
    createdAt: Date;
    items: OrderItem[];
}

export interface CreateOrderItem {
    skuId: number;
    quantity: number;
}

export interface CreateOrderRequest {
    storeId: number;
    items: CreateOrderItem[];
}

export interface InventoryItem {
    skuId: number;
    skuCode: string;
    productName: string;
    onHand: number;
    reserved: number;
    available: number; // Calculated helper
}
