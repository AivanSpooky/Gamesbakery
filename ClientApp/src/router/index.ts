import { createRouter, createWebHistory, type RouteLocationNormalized, type Router } from 'vue-router';
import { useAuthStore } from '@/stores/auth';
const routes = [
  { path: '/', name: 'home', component: () => import('@/views/Home.vue'), meta: { requiresAuth: false } },
  { path: '/account/register', name: 'register', component: () => import('@/views/Register.vue'), meta: { requiresAuth: false } },
  { path: '/account/login', name: 'login', component: () => import('@/views/Login.vue'), meta: { requiresAuth: false } },
  { path: '/user/profile', name: 'profile', component: () => import('@/views/Profile.vue'), meta: { requiresAuth: true } },
  { path: '/game/index', name: 'games', component: () => import('@/views/Games.vue'), meta: { requiresAuth: false } },
  { path: '/game/details/:id', name: 'game-details', component: () => import('@/views/GameDetails.vue'), meta: { requiresAuth: false } },
  { path: '/cart/index', name: 'cart', component: () => import('@/views/Cart.vue'), meta: { requiresAuth: true } },
  { path: '/order/index', name: 'orders', component: () => import('@/views/Orders.vue'), meta: { requiresAuth: true } },
  { path: '/game/details/:id/reviews', name: 'reviews', component: () => import('@/views/Reviews.vue'), meta: { requiresAuth: false } },
  { path: '/review/create', name: 'create-review', component: () => import('@/views/CreateReview.vue'), meta: { requiresAuth: true } },
  { path: '/order/details/:id', name: 'order-details', component: () => import('@/views/OrderDetails.vue'), meta: { requiresAuth: true } },
  { path: '/seller/orderitems', name: 'create-key', component: () => import('@/views/CreateKey.vue'), meta: { requiresAuth: true, roles: ['Admin', 'Seller'] } },
  { path: '/gift/create', name: 'create-gift', component: () => import('@/views/CreateGift.vue'), meta: { requiresAuth: true } },
  { path: '/gift/index', name: 'gifts', component: () => import('@/views/Gifts.vue'), meta: { requiresAuth: true } },
  { path: '/admin/users', name: 'admin-users', component: () => import('@/views/AdminUsers.vue'), meta: { requiresAuth: true, roles: ['Admin'] } },
];
const router: Router = createRouter({
  history: createWebHistory(),
  routes,
});
router.beforeEach((to: RouteLocationNormalized, from: RouteLocationNormalized, next) => {
  const authStore = useAuthStore();
  if (!to.meta.requiresAuth) {
    next();
    return;
  }
  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    next('/account/login');
    return;
  }
  if (to.meta.roles && !(to.meta.roles as string[]).includes(authStore.role)) {
    next('/');
    return;
  }
  next();
});
export default router;
