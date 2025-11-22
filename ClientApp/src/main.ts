import { createApp } from 'vue';
import { createPinia } from 'pinia';
import App from './App.vue';
import router from './router';
import NotificationSuccess from './components/ui/notifications/NotificationSuccess.vue';
import NotificationWarning from '@/components/ui/notifications/NotificationWarning.vue';
import NotificationError from '@/components/ui/notifications/NotificationError.vue';
import './assets/base.css'; // Changed to SCSS

const app = createApp(App);
app.component('notification-success', NotificationSuccess);
app.component('notification-warning', NotificationWarning);
app.component('notification-error', NotificationError);
app.use(createPinia());
app.use(router);
app.mount('#app');
