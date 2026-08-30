import { api } from './client.js'

/*
 * ==========================================
 * FUNCTION: getProducts
 * ==========================================
 * PURPOSE:
 * Fetches the complete catalog of products from the server.
 * 
 * SOURCE & DATA FLOW:
 * - Called automatically on page load in Order.jsx and Products.jsx.
 * - Endpoint: GET /products
 * 
 * RETURNS:
 * An array of product objects containing id, name, price, category, unitType, and availability.
 */
export function getProducts() {
  return api.get('/products')
}

/*
 * ==========================================
 * FUNCTION: createProduct
 * ==========================================
 * PURPOSE:
 * Adds a new product item to the store catalog.
 * 
 * SOURCE & DATA FLOW:
 * - Triggered by Admin action via the creation modal in Products.jsx.
 * - Endpoint: POST /products
 * - Payload: { name, price, unitType, category }
 * 
 * RETURNS:
 * The newly created product object from the database.
 */
export function createProduct(data) {
  return api.post('/products', data)
}

/*
 * ==========================================
 * FUNCTION: updateProduct
 * ==========================================
 * PURPOSE:
 * Updates all fields of an existing product by its ID.
 * 
 * SOURCE & DATA FLOW:
 * - Triggered by Admin action inside the edit modal in Products.jsx.
 * - Endpoint: PUT /products/{id}
 * - Payload: Modified product object fields.
 * 
 * RETURNS:
 * The updated product object.
 */
export function updateProduct(id, data) {
  return api.put(`/products/${id}`, data)
}

/*
 * ==========================================
 * FUNCTION: deleteProduct
 * ==========================================
 * PURPOSE:
 * Deletes a product from the active catalog.
 * 
 * SOURCE & DATA FLOW:
 * - Triggered by Admin after delete confirmation in Products.jsx.
 * - Endpoint: DELETE /products/{id}
 * 
 * NOTE:
 * Existing order histories referencing this product remain intact on the server.
 */
export function deleteProduct(id) {
  return api.del(`/products/${id}`)
}

/*
 * FUNCTION: setAvailability
 * PURPOSE:
 * Toggles a product's stock status between available and out-of-stock.
 * 
 * SOURCE & DATA FLOW:
 * - Triggered when clicking status pills in Products.jsx.
 * - Endpoint: PATCH /products/{id}/availability
 * - Payload: { isAvailable: boolean }
 * 
 * RETURNS:
 * The updated product object with new availability state.
 */
export function setAvailability(id, isAvailable) {
  return api.patch(`/products/${id}/availability`, { isAvailable })
}