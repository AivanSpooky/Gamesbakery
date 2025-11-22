<template>
  <div :class="['profile-btn', className]" @click="navigateToProfile">
    <UserAlt class="user-alt" />
  </div>
</template>
<script lang="ts">
  import { defineComponent } from 'vue';
  import { useRouter } from 'vue-router'; // Added import
  import UserAlt from '@/components/icons/UserAlt.vue';
  export default defineComponent({
    name: 'ProfileBtn',
    components: { UserAlt },
    props: {
      className: { type: String, default: '' },
    },
    setup() {
      const router = useRouter(); // Use useRouter
      const navigateToProfile = () => {
        router.push('/user/profile');
      };
      return { navigateToProfile };
    },
  });
</script>

<style scoped>
  .profile-btn {
    background-color: var(--color-background-accent);
    border: var(--border-interactive) solid var(--color-border-1);
    border-radius: var(--radius-main);
    display: flex;
    align-items: center;
    justify-content: center;
    height: 120px;
    width: 115px;
    overflow: hidden;
    cursor: pointer;
  }

  .user-alt {
    height: 72px;
    width: 72px;
  }
</style>
