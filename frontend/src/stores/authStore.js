import { authService } from "@/services/auth";
import { defineStore } from "pinia";
import { computed, ref } from "vue";
export const useAuthStore = defineStore("auth", () => {
  const savedUser = localStorage.getItem("user");
  const user = ref(savedUser ? JSON.parse(savedUser) : null);
  const isAuthenticated = ref(!!savedUser);
  // Chỉ dùng để ẩn/hiện giao diện; backend vẫn kiểm tra quyền ở mọi request
  const isAdmin = computed(() => !!user.value?.positions?.includes("Admin"));
  const setAuthData = (userData) => {
    isAuthenticated.value = true;
    user.value = userData;
    localStorage.setItem("user", JSON.stringify(userData));
  };
  const setUser = (userData) => {
    user.value = userData;
    localStorage.setItem("user", JSON.stringify(userData));
  };

  // Xoá phiên ở phía client (không gọi API). Dùng khi phiên hết hạn hoặc bị thu hồi.
  const clearSession = () => {
    user.value = null;
    isAuthenticated.value = false;
    localStorage.removeItem("user");
  };

  const logout = async () => {
    try {
      await authService.logout();
    } catch (error) {
      console.error("Lỗi khi đăng xuất backend:", error);
    } finally {
      clearSession();
    }
  };
  const clearFirstLoginFlag = () => {
    if (user.value) {
      user.value.isFirstLogin = false;
      localStorage.setItem("user", JSON.stringify(user.value));
    }
  };
  const resetPassword = async (data) => {
    try {
      const response = await authService.resetMyPassword(data);
      clearFirstLoginFlag();
      return response;
    } catch (error) {
      throw error;
    }
  };
  const refreshToken = async () => {
    try {
      const response = await authService.refreshToken();
      return response;
    } catch (error) {
      await logout();
      throw error;
    }
  };
  return {
    user,
    setAuthData,
    setUser,
    logout,
    clearSession,
    resetPassword,
    clearFirstLoginFlag,
    refreshToken,
    isAuthenticated,
    isAdmin,
  };
});
