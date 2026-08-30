import { api } from './client.js'

// Submits a new order containing the customer ID, purchased items list, and optional redeemed loyalty points (defaults to 0)
export function createOrder(customerId, items, pointsRedeemed = 0) {
  return api.post('/orders', { customerId, pointsRedeemed, items })
}

// Triggers the cancellation process for an existing order using its unique ID
export function cancelOrder(id) {
  return api.post(`/orders/${id}/cancel`, {})
}

// Processes a return request for specified item payloads belonging to a target order ID
export function returnItems(id, items) {
  return api.post(`/orders/${id}/returns`, { items })
}

// Fetches the detailed information and summary status for a single order by its ID
export function getOrder(id) {
  return api.get(`/orders/${id}`)
}