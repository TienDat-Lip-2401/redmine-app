import { http } from "@/api/axiosClient";

// Vai trò của thành viên trong dự án (Backend Developer, Tester...)
export const roleService = {
  // GET: api/Role -> [{ id, name, memberCount }]
  getAll: () => http.get("/role"),
  // POST: api/Role { name } (Admin)
  create: (data) => http.post("/role", data),
  // PUT: api/Role/{id} { name } (Admin)
  update: (id, data) => http.put(`/role/${id}`, data),
  // DELETE: api/Role/{id} (Admin)
  remove: (id) => http.delete(`/role/${id}`),
};
