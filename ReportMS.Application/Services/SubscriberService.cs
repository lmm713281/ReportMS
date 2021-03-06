﻿using System;
using System.Collections.Generic;
using System.Linq;
using ReportMS.DataTransferObjects.Dtos;
using ReportMS.Domain.Models.SubscriberModule;
using ReportMS.Domain.Repositories;
using ReportMS.ServiceContracts;

namespace ReportMS.Application.Services
{
    /// <summary>
    /// 订阅器应用服务
    /// </summary>
    public class SubscriberService : ISubscriberService
    {
        private readonly IAttachmentTopicRepository _attachmentTopicRepository;
        private readonly ITaskRecordRepository _taskRecordRepository;
        
        #region Ctor

        public SubscriberService(IAttachmentTopicRepository attachmentTopicRepository,
            ITaskRecordRepository taskRecordRepository)
        {
            _attachmentTopicRepository = attachmentTopicRepository;
            _taskRecordRepository = taskRecordRepository;
        }

        #endregion

        #region ISubscriberService Members

        public IEnumerable<AttachmentTopicDto> FindAttachmentTopics()
        {
            return this._attachmentTopicRepository.FindAll().ToList().MapAs<AttachmentTopicDto>();
        }

        public AttachmentTopicDto FindAttachmentTopic(Guid topicId)
        {
            return this._attachmentTopicRepository.GetByKey(topicId).MapAs<AttachmentTopicDto>();
        }

        public void CreateAttachmentTopic(AttachmentTopicDto topicDto)
        {
            var topic = new AttachmentTopic(topicDto.TopicName, topicDto.Description, 
                topicDto.Subject, topicDto.Body,
                topicDto.ReportId, topicDto.SqlStatement, topicDto.Parameter, topicDto.CreatedBy);
            var taskDtos = topicDto.TopicTasks;
            if (taskDtos != null)
            {
                var tasks = (from task in taskDtos
                     select new TopicTask(topic.ID, (TaskSchedule) task.TaskSchedule, task.Month, task.Week, task.Day, task.Hour));
                topic.AddTopicTasks(tasks.ToArray());
            }

            var subscribers = topicDto.Subscribers;
            if (subscribers != null)
            {
                var taskSubcribers = (from subscriber in subscribers
                    select new Subscriber(topic.ID, subscriber.Email));
                topic.AddSubscribers(taskSubcribers.ToArray());
            }

            this._attachmentTopicRepository.Add(topic);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (this._attachmentTopicRepository != null)
                this._attachmentTopicRepository.Context.Dispose();
            if (this._taskRecordRepository != null)
                this._taskRecordRepository.Context.Dispose();
        }

        #endregion
    }
}
