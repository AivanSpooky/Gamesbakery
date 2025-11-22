<template>
  <div class="login">
    <div class="welcome-screen">
      <BigTitle class="big-title" text="Вход" />
      <form @submit.prevent="handleLogin" class="input-layout">
        <InputExample class="input-field"
                      inputLblText="Логин:"
                      v-model="username"
                      placeholder="Введите логин" />
        <InputExample class="input-field"
                      inputLblText="Пароль:"
                      type="password"
                      v-model="password"
                      placeholder="Введите пароль" />
        <ActionsExample class="actions"
                        regLblText="Войти"
                        haveAccLblText="У меня нет аккаунта"
                        @haveAccClick="router.push('/account/register')" />
      </form>
    </div>
  </div>
</template>
<script setup lang="ts">
  import { useRouter } from 'vue-router';
  import BigTitle from '@/components/ui/titles/BigTitle.vue';
  import InputExample from '@/components/ui/InputExample.vue';
  import ActionsExample from '@/components/ui/ActionsExample.vue';
  import { useLogin } from '@/composables/useAuth';
  const router = useRouter();
  const { username, password, handleLogin } = useLogin();
</script>

<style scoped>
  .login {
    width: 100vw;
    min-height: 100vh;
    display: flex;
    justify-content: center;
    align-items: center;
    background-color: var(--color-text-primary);
    padding: 0; /* Remove padding to allow full stretch */
    margin: 0;
  }

  .welcome-screen {
    background-color: var(--color-background);
    padding: 40px;
    border-radius: var(--radius-small);
    border: 4px solid var(--color-background-border);
    width: 100vw; /* Full viewport width */
    min-height: 100vh; /* Full height */
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: top; /* Center content vertically */
    box-sizing: border-box;
  }

  .big-title {
    color: var(--color-text-primary);
    font-size: 2.5rem;
    margin-bottom: 30px;
    text-align: center;
  }

  .input-layout {
    display: flex;
    flex-direction: column;
    gap: 20px;
    width: 100%; /* Full width of parent */
    padding: 0 0px; /* Add side padding for form */
    box-sizing: border-box;
  }

  .input-field {
    width: 100%;
  }

  .actions {
    display: flex;
    justify-content: center;
    gap: 20px;
  }
  /* Media query for larger screens */
  @media (min-width: 768px) {
    .input-layout {
      max-width: 1400px; /* Optional: Limit form width on desktops for better UX */
    }
  }
</style>
