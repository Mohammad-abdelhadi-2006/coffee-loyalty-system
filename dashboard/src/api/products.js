import { api } from './client.js'

export function getProducts() {
  return api.get('/products')
}

export function createProduct(data) {
  return api.post('/products', data)
}

export function updateProduct(id, data) {
  return api.put(`/products/${id}`, data)
}

export function deleteProduct(id) {
  return api.del(`/products/${id}`)
}

export function setAvailability(id, isAvailable) {
  return api.patch(`/products/${id}/availability`, { isAvailable })
}