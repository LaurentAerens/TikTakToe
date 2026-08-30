import { useCallback } from 'react';
import { Toast, ToastBody, ToastTitle, useToastController } from '@fluentui/react-components';

/** Matches the <Toaster toasterId="app-toaster" /> mounted in App.tsx. */
const TOASTER_ID = 'app-toaster';

/** Surfaces an API failure to the user instead of letting it fail silently. */
export function useApiErrorToast(): (title: string, error: unknown) => void {
  const { dispatchToast } = useToastController(TOASTER_ID);

  return useCallback(
    (title: string, error: unknown) => {
      const message = error instanceof Error ? error.message : String(error);

      dispatchToast(
        <Toast>
          <ToastTitle>{title}</ToastTitle>
          <ToastBody>{message}</ToastBody>
        </Toast>,
        { intent: 'error' },
      );
    },
    [dispatchToast],
  );
}
