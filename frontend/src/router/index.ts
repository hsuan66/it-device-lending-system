import { createRouter, createWebHistory } from 'vue-router'

/**
 * 擴充路由的 meta 型別，讓每個頁面可以設定 title
 */
declare module 'vue-router' {
  interface RouteMeta {
    /** 頁面名稱，會顯示在瀏覽器標籤，例如「設備管理」 */
    title?: string
  }
}

/**
 * 系統名稱，放在每個頁面名稱前面
 */
const appTitle = 'IT 設備借用系統'

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
      meta: { title: '設備管理' },
    },
    {
      path: '/borrow-records',
      name: 'borrow-records',
      component: () => import('@/views/BorrowRecordView.vue'),
      meta: { title: '借用紀錄' },
    },
    {
      // 找不到的網址一律回到設備管理
      path: '/:pathMatch(.*)*',
      redirect: '/equipment',
    },
  ],
})

// 每次切換頁面後，更新瀏覽器標籤的標題，例如「IT 設備借用系統｜設備管理」
router.afterEach((to) => {
  document.title = to.meta.title ? `${appTitle}｜${to.meta.title}` : appTitle
})

export default router
