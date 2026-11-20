import { useAuthStore } from "@/stores/authStore";
import { ROUTE_NAMES } from "../constants/routeNames";
export const authGuard = (to, from) => {
  const authStore = useAuthStore();
  const publicRoutes = [ROUTE_NAMES.LOGIN, ROUTE_NAMES.FORGOT_PASSWORD];
  const isPublic = publicRoutes.includes(to.name);
  const isAuthenticated = authStore.isAuthenticated;
  if (isPublic) {
    if (isAuthenticated) {
      return { name: ROUTE_NAMES.HOME };
    } else {
      return;
    }
  } else {
    if (!isAuthenticated) {
      return { name: ROUTE_NAMES.LOGIN };
    }
    // Trang chỉ dành cho Admin: người khác quay về Home (backend vẫn chặn API bằng 403)
    if (to.matched.some((record) => record.meta.adminOnly) && !authStore.isAdmin) {
      return { name: ROUTE_NAMES.HOME };
    }
    return;
  }
};
