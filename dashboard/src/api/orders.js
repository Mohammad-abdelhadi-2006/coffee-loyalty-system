import { api } from './client.js'

export function createOrder(customerId, items, pointsRedeemed = 0) {
  return api.post('/orders', { customerId, pointsRedeemed, items })
}

export function cancelOrder(id) {
  return api.post(`/orders/${id}/cancel`, {})
}

export function returnItems(id, items) {
  return api.post(`/orders/${id}/returns`, { items })
}

export function getOrder(id) {
  return api.get(`/orders/${id}`)
}