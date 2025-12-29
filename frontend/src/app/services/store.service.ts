import { Injectable, signal, computed } from '@angular/core';

/**
 * Service responsible for managing the global application state regarding the selected Store Context.
 * Uses Angular Signals to provide reactive state management across components.
 * 
 * Key Responsibilities:
 * - Persisting the selected Store ID in LocalStorage (for session continuity).
 * - Exposing the current User Role (RBAC simulation).
 * - Providing a centralized source of truth for the active Store entity.
 */
@Injectable({
  providedIn: 'root'
})
export class StoreService {
  private readonly STORE_KEY = 'retailops_selected_store';
  private readonly ROLE_KEY = 'retailops_user_role';
  
  /** Signal holding the currently selected Store ID. Default is retrieved from storage or 1. */
  selectedStoreId = signal<number>(this.getStoredId());

  /** Signal holding the current user's role (admin, manager, clerk). Used for UI permission checks. */
  userRole = signal<string>(this.getStoredRole());
  
  /** Cache of all available stores to allow lookup by ID without extra API calls. */
  stores = signal<any[]>([]);
  
  /** 
   * Computed Signal that returns the full Store object corresponding to the selected ID.
   * Updates automatically whenever `stores` or `selectedStoreId` changes.
   */
  selectedStore = computed(() => 
    this.stores().find(s => s.id === this.selectedStoreId())
  );

  /**
   * Updates the selected store and persists it to LocalStorage.
   * @param id The ID of the store to switch to.
   */
  setStore(id: number) {
    if (typeof localStorage !== 'undefined') {
      localStorage.setItem(this.STORE_KEY, id.toString());
    }
    this.selectedStoreId.set(id);
  }

  /**
   * Updates the current user role and persists it (Demo purposes).
   * @param role The role to assign ('admin', 'manager', 'clerk').
   */
  setRole(role: string) {
    if (typeof localStorage !== 'undefined') {
      localStorage.setItem(this.ROLE_KEY, role);
    }
    this.userRole.set(role);
  }

  private getStoredId(): number {
    if (typeof localStorage !== 'undefined') {
      const stored = localStorage.getItem(this.STORE_KEY);
      return stored ? parseInt(stored, 10) : 1;
    }
    return 1;
  }

  private getStoredRole(): string {
    if (typeof localStorage !== 'undefined') {
      return localStorage.getItem(this.ROLE_KEY) || 'admin'; // Default to admin for demo
    }
    return 'admin';
  }
}
