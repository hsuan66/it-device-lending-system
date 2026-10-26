import { createRouter, createWebHistory } from 'vue-router'

/**
 * 前端頁面路由，頁面元件在第一次進入時才載入
 */
const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      redirect: '/equipment',
    },
    {
      path: '/equipment',
      name: 'equipment',
      component: () => import('@/views/EquipmentView.vue'),
    },
    {
      path: '/borrow-records',
      name: 'borrow-records',
      component: () => import('@/views/BorrowRecordView.vue'),
    },
    {
      // 找不到的網址一律回到設備管理
      path: '/:pathMatch(.*)*',
      redirect: '/equipment',
    },
  ],
})

export default router
