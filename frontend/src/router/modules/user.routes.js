import { ROUTE_NAMES } from "../constants/routeNames";
export const userRoutes = [
  {
    path: "/users",
    component: () => import("@/layouts/DefaultLayout/DefaultLayout.vue"),
    meta: { requiresAuth: true },
    children: [
      {
        path: "",
        name: ROUTE_NAMES.USER_LIST,
        component: () => import("@/pages/user/UsersPage.vue"),
        meta: { adminOnly: true },
      },
      {
        path: "create",
        name: ROUTE_NAMES.CREATE_USER,
        component: () => import("@/pages/user/CreateUserPage.vue"),
        meta: { adminOnly: true },
      },
      {
        path: "update/:id",
        name: ROUTE_NAMES.UPDATE_USER,
        component: () => import("@/pages/user/UpdateUserPage.vue"),
        meta: { adminOnly: true },
      },
      {
        path: "my-profile",
        name: ROUTE_NAMES.PROFILE_USER,
        component: () => import("@/pages/user/ProfilePage.vue"),
      },
    ],
  },
];
