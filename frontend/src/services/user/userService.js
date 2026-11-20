import { http } from "@/api/axiosClient";
export const userService = {
  // Danh sách đầy đủ, dùng cho ô chọn người dùng (vd. modal thêm thành viên)
  getAll: () => {
    return http.get("/user/get-all");
  },
  // Có phân trang: params = { page, size, field, keyword, status }
  // Trả về ApiResponse.data = { data: [...], totalItems, totalPages, currentPage, pageSize }
  getPaged: (params) => {
    return http.get("/user", { params });
  },
  delete: (id) => {
    return http.delete(`/user/${id}`);
  },
  update: (id, data) => {
    return http.put(`/user/${id}`, data);
  },
  checkEmail: (email) => {
    return http.get(`/user/check-email?email=${encodeURIComponent(email)}`);
  },
  create: (data) => {
    return http.post("/user/create", data);
  },
  getById: (id) => {
    return http.get(`/user/${id}`);
  },
  // User đang hoạt động có chức vụ Project Manager (Admin, PM)
  getProjectManagers: () => {
    return http.get("/user/project-managers");
  },
  getMyProfile: () => {
    return http.get(`user/my-profile`);
  },
};
