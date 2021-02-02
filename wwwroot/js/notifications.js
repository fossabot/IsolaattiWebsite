function deleteNotifications() {
    if(confirm("Do you really want to delete all notifications?")) {
        let formData = new FormData();
        formData.append("userId", userData.id);
        formData.append("password", userData.password);
        
        let request = new XMLHttpRequest();
        request.open("POST", "/api/DeleteNotification/All");
        request.send(formData);
        request.onreadystatechange = function() {
            if(request.readyState === XMLHttpRequest.DONE) {
                if(!(request.status === 403 || request.status === 404)) {
                    window.location.reload();
                } else {
                    alert("An error deleting notifications occurred");
                }
            }
        }
    }
}

function deleteNotification(id) {
    if(confirm("Do you really want to delete this notification?")) {
        let formData = new FormData();
        formData.append("userId", userData.id);
        formData.append("password", userData.password);
        formData.append("notificationId", id);

        let request = new XMLHttpRequest();
        request.open("POST", "/api/DeleteNotification");
        request.send(formData);
        request.onreadystatechange = function() {
            if(request.readyState === XMLHttpRequest.DONE) {
                if(!(request.status === 403 || request.status === 404)) {
                    window.location.reload();
                } else {
                    alert("An error deleting notifications occurred");
                }
            }
        }
    }
}