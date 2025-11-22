import { ref, onMounted } from 'vue';
import { GiftService } from '@/services/GiftService';
import { useNotifications } from '@/composables/useNotifications';
import type { Gift } from '@/types/gift';

export function useGiftsList() {
  const gifts = ref<Gift[]>([]);
  const filterType = ref('all');
  const { showError } = useNotifications();

  const loadGifts = async () => {
    try {
      gifts.value = await GiftService.getAll(filterType.value);
    } catch (err: any) {
      showError('Ошибка загрузки подарков');
    }
  };

  const applyFilter = ({ type }: { type: string }) => {
    filterType.value = type;
    loadGifts();
  };

  onMounted(loadGifts);
  return { gifts, applyFilter };
}
