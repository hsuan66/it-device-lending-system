import { createApp } from 'vue'
import { createPinia } from 'pinia'
import ElementPlus from 'element-plus'
import zhTw from 'element-plus/es/locale/lang/zh-tw'
import 'element-plus/dist/index.css'

import App from './App.vue'
import router from './router'

const app = createApp(App)

app.use(createPinia())
app.use(router)

// Element Plus：元件內建文字（例如分頁、日期選擇器、確認框按鈕）使用繁體中文
app.use(ElementPlus, { locale: zhTw })

app.mount('#app')
