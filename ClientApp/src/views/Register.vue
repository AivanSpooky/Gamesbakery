<template>
  <div class="register">
    <div class="welcome-screen">
      <BigTitle class="big-title" text="Регистрация" />
      <form @submit.prevent="handleRegister" class="input-layout">
        <InputExample class="input-field"
                      inputLblText="Имя пользователя:"
                      v-model="username"
                      placeholder="Введите имя пользователя" />
        <InputExample class="input-field"
                      inputLblText="Электронная почта:"
                      type="email"
                      v-model="email"
                      placeholder="Введите email" />
        <InputExample class="input-field"
                      inputLblText="Пароль:"
                      type="password"
                      v-model="password"
                      placeholder="Введите пароль" />
        <InputExample class="input-field"
                      inputLblText="Страна:"
                      v-model="country"
                      placeholder="Введите страну" />
        <ActionsExample class="actions"
                        regLblText="Зарегистрироваться"
                        haveAccLblText="У меня уже есть аккаунт"
                        @regClick="handleRegister"
                        @haveAccClick="router.push('/account/login')" />
      </form>
    </div>
  </div>
</template>
<script setup lang="ts">
  import { useRouter } from 'vue-router';
  import BigTitle from '@/components/ui/titles/BigTitle.vue';
  import InputExample from '@/components/ui/InputExample.vue';
  import ActionsExample from '@/components/ui/ActionsExample.vue';
  import { useRegister } from '@/composables/useAuth';
  const router = useRouter();
  const { username, email, password, country, handleRegister } = useRegister();
</script>

<style scoped>
  .register {
    width: 100vw;
    min-height: 100vh;
    display: flex;
    justify-content: center;
    align-items: center;
    background-color: #ffffff;
    padding: 0;
    margin: 0;
  }

  .welcome-screen {
    background-color: #001199;
    padding: 40px;
    border-radius: 20px;
    border: 4px solid #00d8ff;
    width: 100vw;
    min-height: 100vh;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: flex-start;
    box-sizing: border-box;
  }

  .big-title {
    color: #ffffff;
    font-size: 2.5rem;
    margin-bottom: 30px;
    text-align: center;
  }

  .input-layout {
    display: flex;
    flex-direction: column;
    gap: 20px;
    width: 100%;
    padding: 0 20px;
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

  @media (min-width: 768px) {
    .input-layout {
      max-width: 1400px;
    }
  }
</style>
