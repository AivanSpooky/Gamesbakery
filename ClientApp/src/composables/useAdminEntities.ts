import { ref, computed, onMounted } from 'vue';
import { UserService } from '@/services/UserService';
import { SellerService } from '@/services/SellerService';
import { useNotifications } from '@/composables/useNotifications';
import type { AdminEntity } from '@/types/admin';

export function useAdminEntities() {
  const entities = ref<AdminEntity[]>([]);
  const isLoading = ref(true);
  const filterType = ref('Все');
  const filterName = ref('');
  const { showError } = useNotifications();

  const filteredEntities = computed(() => entities.value.filter(entity => {
    const typeMatch = filterType.value === 'Все' || entity.type === filterType.value;
    const nameMatch = entity.username.toLowerCase().includes(filterName.value.toLowerCase());
    return typeMatch && nameMatch;
  }));

  const applyFilters = async ({ type, name }: { type: string; name: string }) => {
    filterType.value = type;
    filterName.value = name;
    // Optionally refetch if server-side filtering is implemented; for now, client-side
  };

  const loadAll = async () => {
    isLoading.value = true;
    try {
      const users: any[] = await UserService.getAllUsers(1, 1000, true);  // Use getAll=true
      const sellers: any[] = await SellerService.getAllSellers(1, 1000, true);
      entities.value = [
        ...users.map((u: any) => ({ ...u, type: 'User', username: u.username, email: u.email, isBlocked: u.isBlocked || false })),
        ...sellers.map((s: any) => ({ id: s.id, username: s.sellerName, role: 'Seller', isBlocked: false, avgRating: s.avgRating, type: 'Seller' })),
      ];
    } catch (error: any) {
      showError('Ошибка загрузки списка пользователей и продавцов');
    } finally {
      isLoading.value = false;
    }
  };

  onMounted(loadAll);

  return { filteredEntities, isLoading, applyFilters };
}
