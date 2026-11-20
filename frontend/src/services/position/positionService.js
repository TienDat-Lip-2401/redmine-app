import { http } from "@/api/axiosClient";

export const positionService = {
  // GET: api/Position/user/{userId}
  getByUserId: (userId) => {
    return http.get(`/position/user/${userId}`);
  },

  // GET: api/Position
  getAllByCurrentUser: () => {
    return http.get("/position");
  },
  getAllPosition: () => {
    return http.get("/position/all");
  },
  // POST: api/Position { name } (Admin)
  create: (data) => http.post("/position", data),
  // PUT: api/Position/{id} { name } (Admin, không áp dụng cho chức vụ hệ thống)
  update: (id, data) => http.put(`/position/${id}`, data),
  // DELETE: api/Position/{id} (Admin, không áp dụng cho chức vụ hệ thống)
  remove: (id) => http.delete(`/position/${id}`),
  // GET: api/Position/available/{userId}
  getAvailable: (userId) => {
    return http.get(`/position/available/${userId}`);
  },

  // POST: api/Position/assign-list
  // DTO: AssignPositionListRequestDto { userId, positionIds }
  addPositions: (data) => {
    return http.post("/position/assign-list", data);
  },

  // DELETE: api/Position/remove-position
  // DTO: DeletePositionRequest { userId, positionId }
  removePosition: (payload) => {
    return http.delete("/position/remove-position", {
      data: {
        userId: payload.userId,
        positionId: payload.positionId,
      },
    });
  },
};
