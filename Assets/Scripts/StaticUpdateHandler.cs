public class StaticUpdateHandler : Singleton<StaticUpdateHandler> {

    private Callback onLateUpdate;

    public void Subscribe_LateUpdate(Callback subscriber) {
        onLateUpdate += subscriber;
    }

    public void Unsubscribe_LateUpdate(Callback subscriber) {
        onLateUpdate -= subscriber;
    }

    private void LateUpdate() {
        if(onLateUpdate == null) {
            return;
        }

        onLateUpdate();
    }
}
