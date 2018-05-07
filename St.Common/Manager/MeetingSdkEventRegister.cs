using Caliburn.Micro;
using MeetingSdk.NetAgent;
using MeetingSdk.NetAgent.Models;
using MeetingSdk.Wpf;
using Prism.Events;

namespace St.Common.Manager
{
    public class MeetingSdkEventsRegister
    {
        private MeetingSdkEventsRegister()
        {

        }

        public static MeetingSdkEventsRegister Instance = new MeetingSdkEventsRegister();

        private readonly IEventAggregator _eventAggregator = IoC.Get<IEventAggregator>();

        public void RegisterSdkEvents()
        {
            IMeetingSdkAgent sdkWrapper = IoC.Get<IMeetingSdkAgent>();

            sdkWrapper.OnStartSpeakEvent = OnStartSpeakEvent;
            sdkWrapper.OnStopSpeakEvent = OnStopSpeakEvent;

            sdkWrapper.OnUserStartSpeakEvent = OnUserStartSpeakEvent;
            sdkWrapper.OnUserStopSpeakEvent = OnUserStopSpeakEvent;

            sdkWrapper.OnUserJoinEvent = OnUserJoinEvent;
            sdkWrapper.OnUserLeaveEvent = OnUserLeaveEvent;

            sdkWrapper.OnUserPublishCameraVideoEvent = OnUserPublishCameraVideoEvent;
            sdkWrapper.OnUserPublishDataVideoEvent = OnUserPublishDataVideoEvent;
            sdkWrapper.OnUserPublishMicAudioEvent = OnUserPublishMicAudioEvent;
            sdkWrapper.OnUserUnpublishCameraVideoEvent = OnUserUnpublishCameraVideoEvent;
            sdkWrapper.OnUserUnpublishDataCardVideoEvent = OnUserUnpublishDataCardVideoEvent;
            sdkWrapper.OnUserUnpublishMicAudioEvent = OnUserUnpublishMicAudioEvent;

            sdkWrapper.OnNetworkStatusLevelNoticeEvent = OnNetworkStatusLevelNoticeEvent;
            sdkWrapper.OnNetworkCheckedEvent = OnNetworkCheckedEvent;

            sdkWrapper.OnRaiseHandRequestEvent = OnRaiseHandRequestEvent;
            sdkWrapper.OnTransparentMsgReceivedEvent = OnTransparentMsgReceivedEvent;
            sdkWrapper.OnUiTransparentMsgReceivedEvent = OnUiTransparentMsgReceivedEvent;
            sdkWrapper.OnHostOrderDoOpratonEvent = OnHostOrderDoOpratonEvent;

            sdkWrapper.OnHostKickoutUserEvent = OnHostKickoutUserEvent;

            sdkWrapper.OnDeviceLostNoticeEvent = OnDeviceLostNoticeEvent;
            sdkWrapper.OnDeviceStatusChangedEvent = OnDeviceStatusChangedEvent;
            sdkWrapper.OnMeetingManageExceptionEvent = OnMeetingManageExceptionEvent;
            sdkWrapper.OnSdkCallbackEvent = OnSdkCallbackEvent;
            sdkWrapper.OnLockStatusChangedEvent = OnLockStatusChangedEvent;

            sdkWrapper.OnMeetingInviteEvent = OnMeetingInviteEvent;
            sdkWrapper.OnContactRecommendEvent = OnContactRecommendEvent;
            sdkWrapper.OnForcedOfflineEvent = OnForcedOfflineEvent;

        }

        private void OnForcedOfflineEvent(MeetingResult<ForcedOfflineModel> obj)
        {
            _eventAggregator.GetEvent<ForcedOfflineEvent>().Publish(obj.Result);
        }

        private void OnContactRecommendEvent(MeetingResult<RecommendContactModel> obj)
        {
            _eventAggregator.GetEvent<ContactRecommendEvent>().Publish(obj.Result);
        }

        private void OnMeetingInviteEvent(MeetingResult<MeetingInvitationModel> obj)
        {
            _eventAggregator.GetEvent<MeetingInvitationEvent>().Publish(obj.Result);
        }

        private void OnUiTransparentMsgReceivedEvent(MeetingResult<UiTransparentMsg> obj)
        {
            _eventAggregator.GetEvent<UiTransparentMsgReceivedEvent>().Publish(obj.Result);
        }

        private void OnLockStatusChangedEvent(MeetingResult obj)
        {
            _eventAggregator.GetEvent<LockStatusChangedEvent>().Publish(obj);
        }

        private void OnSdkCallbackEvent(MeetingResult<SdkCallbackModel> obj)
        {
            _eventAggregator.GetEvent<SdkCallbackEvent>().Publish(obj.Result);
        }

        private void OnMeetingManageExceptionEvent(MeetingResult<ExceptionModel> obj)
        {
            _eventAggregator.GetEvent<MeetingManageExceptionEvent>().Publish(obj.Result);
        }

        private void OnDeviceStatusChangedEvent(MeetingResult<DeviceStatusModel> obj)
        {
            _eventAggregator.GetEvent<DeviceStatusChangedEvent>().Publish(obj.Result);
        }

        private void OnDeviceLostNoticeEvent(MeetingResult<ResourceModel> obj)
        {
            _eventAggregator.GetEvent<DeviceLostNoticeEvent>().Publish(obj.Result);
        }

        private void OnHostKickoutUserEvent(MeetingResult<KickoutUserModel> obj)
        {
            _eventAggregator.GetEvent<HostKickoutUserEvent>().Publish(obj.Result);
        }

        private void OnHostOrderDoOpratonEvent(MeetingResult<HostOprateType> obj)
        {
            _eventAggregator.GetEvent<HostOperationReceivedEvent>().Publish(obj.Result);
        }

        private void OnTransparentMsgReceivedEvent(MeetingResult<TransparentMsg> obj)
        {
            _eventAggregator.GetEvent<TransparentMsgReceivedEvent>().Publish(obj.Result);
        }

        private void OnRaiseHandRequestEvent(MeetingResult<AccountModel> obj)
        {
            _eventAggregator.GetEvent<UserRaiseHandRequestEvent>().Publish(obj.Result);
        }

        private void OnNetworkCheckedEvent(MeetingResult<NetType> obj)
        {
            _eventAggregator.GetEvent<NetCheckedEvent>().Publish(obj.Result);
        }

        private void OnNetworkStatusLevelNoticeEvent(MeetingResult<int> obj)
        {
            _eventAggregator.GetEvent<NetStatusNoticeEvent>().Publish(obj.Result);
        }

        private void OnUserLeaveEvent(MeetingResult<AccountModel> obj)
        {
            _eventAggregator.GetEvent<UserLeaveEvent>().Publish(obj.Result);
        }

        private void OnUserJoinEvent(MeetingResult<AccountModel> obj)
        {
            _eventAggregator.GetEvent<UserJoinEvent>().Publish(obj.Result);
        }

        private void OnUserStopSpeakEvent(MeetingResult<UserSpeakModel> obj)
        {
            _eventAggregator.GetEvent<UserStopSpeakEvent>().Publish(obj.Result);
        }

        private void OnUserStartSpeakEvent(MeetingResult<UserSpeakModel> obj)
        {
            _eventAggregator.GetEvent<UserStartSpeakEvent>().Publish(obj.Result);
        }

        private void OnStopSpeakEvent(MeetingResult<SpeakModel> obj)
        {
            _eventAggregator.GetEvent<StopSpeakEvent>().Publish(obj.Result);

        }

        private void OnStartSpeakEvent(MeetingResult<SpeakModel> taskResult)
        {
            _eventAggregator.GetEvent<StartSpeakEvent>().Publish(taskResult.Result);
        }


        private void OnUserUnpublishMicAudioEvent(MeetingResult<UserUnpublishModel> taskResult)
        {
            _eventAggregator.GetEvent<UserUnpublishMicAudioEvent>().Publish(taskResult.Result);
        }

        private void OnUserUnpublishDataCardVideoEvent(MeetingResult<UserUnpublishModel> taskResult)
        {
            _eventAggregator.GetEvent<UserUnpublishDataCardVideoEvent>().Publish(taskResult.Result);
        }

        private void OnUserUnpublishCameraVideoEvent(MeetingResult<UserUnpublishModel> taskResult)
        {
            _eventAggregator.GetEvent<UserUnpublishCameraVideoEvent>().Publish(taskResult.Result);
        }

        private void OnUserPublishMicAudioEvent(MeetingResult<UserPublishModel> taskResult)
        {
            _eventAggregator.GetEvent<UserPublishMicAudioEvent>().Publish(taskResult.Result);
        }

        private void OnUserPublishDataVideoEvent(MeetingResult<UserPublishModel> taskResult)
        {
            _eventAggregator.GetEvent<UserPublishDataVideoEvent>().Publish(taskResult.Result);
        }

        private void OnUserPublishCameraVideoEvent(MeetingResult<UserPublishModel> taskResult)
        {
            _eventAggregator.GetEvent<UserPublishCameraVideoEvent>().Publish(taskResult.Result);
        }

    }
}
