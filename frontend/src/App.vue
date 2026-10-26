<script setup lang="ts">
import { RouterView, useRoute } from 'vue-router'

/**
 * 目前的路由，用來讓選單標示目前所在的頁面
 */
const route = useRoute()
</script>

<template>
  <el-container class="layout">
    <el-header class="layout-header">
      <div class="layout-title">IT 設備借用系統</div>
      <!-- router：點選單項目時，自動切換到 index 指定的路徑 -->
      <el-menu mode="horizontal" router :default-active="route.path" :ellipsis="false" class="layout-menu">
        <el-menu-item index="/equipment">設備管理</el-menu-item>
        <el-menu-item index="/borrow-records">借用紀錄</el-menu-item>
      </el-menu>
    </el-header>

    <el-main class="layout-main">
      <RouterView />
    </el-main>
  </el-container>
</template>

<style>
body {
  margin: 0;
  background-color: #f5f7fa;
  font-family: 'Noto Sans TC', 'Microsoft JhengHei', sans-serif;
}
</style>

<style scoped>
.layout {
  min-height: 100vh;
}

.layout-header {
  display: flex;
  align-items: center;
  gap: 32px;
  background-color: #ffffff;
}

.layout-title {
  font-size: 20px;
  font-weight: 600;
  white-space: nowrap;
}

.layout-menu {
  flex: 1;
  border-bottom: none;
  /* 選單文字從預設的 14px 放大到 16px */
  --el-menu-item-font-size: 16px;
}

/*
 * 被選到的選單：拿掉 Element Plus 預設的底線，以及點擊後（focus）、滑鼠移上去（hover）的背景色
 * 前面多加 .layout-header，讓權重高過 Element Plus 的 hover、focus 規則
 */
.layout-header .layout-menu :deep(.el-menu-item.is-active) {
  border-bottom-color: transparent;
  background-color: transparent;
}

/* 沒被選到的選單：滑鼠移上去（hover）或 focus 時，維持原本的字色，也不顯示背景色 */
.layout-header .layout-menu :deep(.el-menu-item:not(.is-active):hover),
.layout-header .layout-menu :deep(.el-menu-item:not(.is-active):focus) {
  color: var(--el-menu-text-color);
  background-color: transparent;
}

/* 中版本與手機版（寬度 991px 以下）：標題靠最左、選單靠最右 */
@media (max-width: 991px) {
  .layout-header {
    justify-content: space-between;
  }

  .layout-menu {
    flex: none;
  }
}

/* 手機版（寬度 575px 以下）：縮小標題字體與各種間距，避免標題和選單擠不下 */
@media (max-width: 575px) {
  .layout-header {
    gap: 12px;
    padding: 0 12px;
  }

  .layout-title {
    font-size: 16px;
  }

  .layout-menu {
    --el-menu-base-level-padding: 10px;
  }
}

.layout-main {
  max-width: 1200px;
  width: 100%;
  margin: 0 auto;
  box-sizing: border-box;
}
</style>
