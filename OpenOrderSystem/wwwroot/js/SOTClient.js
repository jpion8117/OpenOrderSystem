
const appendAlert = (message, type, id) => {
    const wrapper = document.createElement('div')
    wrapper.innerHTML = [
        `<div id="${id}" class="alert alert-${type} alert-dismissible" role="alert">`,
        `   <div>${message}</div>`,
        '   <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>',
        '</div>'
    ].join('')

    alertPlaceholder.append(wrapper)
}

class notification {
    constructor(src, loops = 1) {
        this.notificationSound = document.createElement("audio")
        this.notificationSound.setAttribute('src', src);
        this.notificationSound.addEventListener('ended', (event) => {
            this.loopCounter++;
            if (this.loopCounter % this.loops != 0) {
                this.notificationSound.play()
            }
            else {
                notification.soundQueue.pop()
                if (notification.soundQueue.length > 0) {
                    notification.soundQueue[0].notificationSound.play();
                }
            }
        })
        this.loops = loops;
    }

    /**
     * @type {Array<notification>}
     */
    static soundQueue = []; 

    /**
     * @type {HTMLAudioElement}
     */
    notificationSound;

    loopCounter = 0;

    /**
     * @type {number}
     */
    loops;

    play() {
        if (notification.soundQueue.length > 0) {
            notification.soundQueue.unshift(this);
        }
        else {
            notification.soundQueue.unshift(this)
            this.notificationSound.play();
        }
    }
}

class notificationCard {
    static idPool = !sessionStorage.getItem("notification_idPool") ? 0 : parseInt(sessionStorage.getItem("notification_idPool"));

    /**
     * @type {notificationCard[]}
     */
    static active = !sessionStorage.getItem("active_notifications") ? [] : JSON.parse(sessionStorage.getItem("active_notifications"));

    id;
    time;
    html;
    silent;

    constructor(id, time, html, silent) {
        this.id = id;
        this.time = new Date(time);
        this.html = html;
        this.silent = silent;
    }

    static post(title, message, silent = false) {
        const id = notificationCard.idPool++;
        const html =
            `
                <div id="notification_${id}" class="notification${silent ? ' silent' : ''}">
                    <div class="notification-title">
                        <span class="fw-bolder">${title} &mdash; <span class="notification-life" data-id="${id}">Just Now</span></span>
                        <button class="btn-notification-dismiss btn-close float-end me-2" data-id="${id}"></button>
                    </div>
                    <div class="notification-message">${message}</div>
                </div>
            `;

        const card = new notificationCard(id, new Date(), html, silent)
        notificationCard.active.push(card);
        sessionStorage.setItem("active_notifications", JSON.stringify(notificationCard.active));
        sessionStorage.setItem("notification_idPool", notificationCard.idPool)
        const element = $.parseHTML(html);
        $(element).on('click', (event) => {
            let id = $(event.target).data('id');
            notificationCard.dismiss(id);
        });
        $('#notifications').prepend(element);
        if (!silent) {
            $('.active-notifications').addClass('fa-shake text-warning');
            terminal_client.alerts.notification.play();
        }
    }

    static dismiss(id) {
        var index = -1;

        for (var i = 0; i < notificationCard.active.length; i++) {
            if (notificationCard.active[i].id == id) {
                index = i;
                break;
            }
        }

        if (index != -1) {
            notificationCard.active.splice(index, 1);
            sessionStorage.setItem("active_notifications", JSON.stringify(notificationCard.active));
            $(`#notification_${id}`).remove();
            let count = 0;
            for (var i = 0; i < notificationCard.active.length; i++) {
                if (!notificationCard.active[i].silent) count++;
            }
            if (count < 1) {
                $('.active-notifications').removeClass('fa-shake text-warning');
            }
        }
    }

    static getNotification(id) {
        let notification = null;
        for (var i = 0; i < notificationCard.active.length; i++) {
            if (notificationCard.active[i].id == id) {
                notification = notificationCard.active[i];
                break;
            }
        }

        return notification;
    }

    static silenceAll() {
        $('.notification').addClass('silent');
        $('.active-notifications').removeClass('fa-shake text-warning');
        for (var i = 0; i < notificationCard.active.length; i++) {
            notificationCard.active[i].silent = true;
        }
        sessionStorage.setItem("active_notifications", JSON.stringify(notificationCard.active));
    }

    static restoreActiveNotifications() {
        for (let i = 0; i < notificationCard.active.length; ++i) {
            $('#notifications').prepend($.parseHTML(notificationCard.active[i].html));
        }

        $('.btn-notification-dismiss').on('click', (event) => {
            let id = $(event.target).data('id');
            notificationCard.dismiss(id);
        });

        $(document).on('notification-update', (event) => {
            $('.notification-life').each((index, element) => {
                const id = $(element).data('id');
                const info = notificationCard.getNotification(id);
                const card = new notificationCard(info.id, info.time, info.html);
                $(element).text(card.getNotificationLife());
            });
        });
    }

    getNotificationLife() {
        let msg = "Just Now"
        let timeNow = new Date();
        const life = Math.round((timeNow.getTime() - this.time.getTime()) / 1000);
        if (life > 3599) {
            let grammer = life < (3600 * 2) ? "hour" : "hours";
            msg = `${Math.floor(life / 3600)} ${grammer} ago.`
        }
        else if (life > 59) {
            let grammer = life < 120 ? "minute" : "minutes";
            msg = `${Math.floor(life / 60)} ${ grammer } ago.`
        }
        else if (life > 30) {
            msg = `${life} seconds ago.`
        }

        return msg
    }
}

class terminal_client {
    /**
     * 
     * @param {string} printerAddr Address of the printer used to print order tickets
     * @param {number} updateSpeed Speed the page will update at
     */
    constructor(printerAddr, updateSpeed = 30000) {
        this.UPDATE_INTERVAL_TIME = updateSpeed;
        
        if ('wakeLock' in navigator) {
            console.log('attempting to aquire screen lock...');
            try {
                navigator.wakeLock.request('screen')
                    .then((lock) => {
                        console.log("screenlock successful...")
                        this.screenlock = lock;
                        document.addEventListener('visibilitychange', async () => {
                            if (this.screenlock !== null && document.visibilityState === 'visible') {
                                console.log("restoring screenlock...")
                                this.screenlock = await navigator.wakeLock.request('screen');
                            }
                        });
                    });
            }
            catch(err) {
                console.log("unable to aquire screen lock");
                console.info(err);
            }
        }
        else {
            console.log('screen lock not supported...');
        }

        const months = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];

        setInterval(() => {
            const dateNow = new Date();
            $('.date').text(`${months[dateNow.getMonth()]} ${dateNow.getDate()}, ${dateNow.getFullYear()}`);
            $('.time').text(dateNow.toLocaleTimeString());
            $('.time-basic').text(`${dateNow.getHours() > 12 ? dateNow.getHours() - 12 : dateNow.getHours()}:${dateNow.getMinutes() < 10 ? "0" + dateNow.getMinutes().toString() : dateNow.getMinutes()} ${dateNow.toLocaleTimeString().substr(dateNow.toLocaleTimeString().length - 2)}`);
            document.dispatchEvent(new Event('notification-update'));
        }, 1000);

        $('#notification_window_close').on('click', terminal_client.closeNotificationWindow);

        if (!printerAddr.includes(':'))
            throw "Printer address must include a host name (or IP address) and a port number separated by a ':'";
        const printerIP = printerAddr.split(':')[0];
        const printerPort = parseInt(printerAddr.split(':')[1]);

        this.PRINTER_ADDRESS.host = printerIP;
        this.PRINTER_ADDRESS.port = printerPort;

        var failedConnect = 0;
        const configureMonitoring = () => {
            console.log("configuring printer");

            var offlineCooldown = 0;
            this.printer.onoffline = this.printer.onpoweroff = function () {
                if (offlineCooldown <= 0) {
                    offlineCooldown = 9000;
                    terminal_client.changePrinterIconStatus("connection", "error", true);
                    terminal_client.changePrinterIconStatus("paper", "default");
                    terminal_client.changePrinterIconStatus("error", "error", true);
                    notificationCard.post("Printer Offline", `Printer signal lost. Printer may be offline or malfunctioning.`);

                }
                else {
                    offlineCooldown--;
                }
            };

            this.printer.oncoveropen = function () {
                terminal_client.changePrinterIconStatus("error", "error", true);
                notificationCard.post("Printer Cover Open", "The printer cover is open, please close it to resume printing.");
            };

            this.printer.oncoverok = () => {
                terminal_client.changePrinterIconStatus("error", "default");
            };

            this.printer.ononline = () => {
                terminal_client.changePrinterIconStatus("connection", "good");
                terminal_client.changePrinterIconStatus("error", "default");
            };

            this.printer.onpapernearend = () => {
                terminal_client.changePrinterIconStatus("paper", "warning", true);
                this.printer.lowPaperWarningSent = new Date();

                if (this.printer.lowPaperWarningSent && new Date() - this.printer.lowPaperWarningSent > 900000) {
                    notificationCard.post("Printer Paper Low", "The printer is low on paper, please change paper soon.", true);
                }
            };

            this.printer.onpaperend = () => {
                terminal_client.changePrinterIconStatus("paper", "error", true);
                notificationCard.post("Printer Paper Out",
                    "The printer has run out of paper. Please replace the paper to continue printing.");
            };

            this.printer.onpaperok = () => {
                terminal_client.changePrinterIconStatus("paper", "default");
            };

            this.printer.onreceive = function (res) {
            };
            
            this.printer.startMonitor();
        }
        const connectCB = (data) => {
            if (data == "SSL_CONNECT_OK") {
                console.log("Printer connected!")
                terminal_client.changePrinterIconStatus('connection', 'good', true);
                this.printer = this.ePosDev.createDevice("local_printer", this.ePosDev.DEVICE_TYPE_PRINTER,
                    {
                        'crypto': true,
                        'buffer': true
                    }, (devobj, retcode) => {
                        if (retcode == "OK") {
                            terminal_client.changePrinterIconStatus('connection', 'good');
                            this.printer = devobj;
                            configureMonitoring();
                        }
                        else {
                            terminal_client.changePrinterIconStatus("connection", "error", true);
                            terminal_client.changePrinterIconStatus("paper", "error", true);
                            terminal_client.changePrinterIconStatus("error", "error", true);
                            notificationCard.post("Unknown Printer Error", `An unknown printer error occured with error: "${retcode}"`)
                        }
                    });
            }
            else {
                console.error(`Printer connection attempt #${failedConnect} result: ${data}`);
                ++failedConnect;
                terminal_client.changePrinterIconStatus('connection', 'warning', true);
                if (failedConnect > 4) {
                    notificationCard.post("Failed to Connect Printer", `System failed to connect to printer at ${this.PRINTER_ADDRESS.host}:${this.PRINTER_ADDRESS.port}. Reload page to try again or contact support.`);
                    terminal_client.changePrinterIconStatus('connection', 'error');
                }
                else {
                    setTimeout(() => {
                        this.ePosDev.connect(printerIP, printerPort, connectCB);
                    }, 5000);
                }
            }
        }

        this.ePosDev.connect(printerIP, printerPort, connectCB);
    }

    /**
     * @type {number} Speed at which the printer will update.
     */
    UPDATE_INTERVAL_TIME;

    screenlock;

    /**
     * @type {Object} Network address of the printer used to print order slips
     */
    PRINTER_ADDRESS = {
        /**
         * @type {string}
         */
        host: "",

        /**
         * @type {number}
         */
        port: 0
    };

    static changePrinterIconStatus(icon, statusClass, blink = false) {
        $(`#printer_${icon}_status`)
            .removeClass('default pending error warning good fa-fade')
            .addClass(blink ? `fa-fade ${statusClass}` : statusClass);
    }
;

    /**
     * @type {?number} Interval timer for the server update loop
     */
    updateTimer;

    /**
     * @type {ePOSDevice} epson ePOS device used for connecting the printer
     */
    ePosDev = new epson.ePOSDevice();

    printer = null;

    loadPrintHeader() {
        this.printer
            .addTextAlign(this.printer.ALIGN_CENTER)
            .addLogo(32, 33)
            .addFeedLine(1)
            .addTextSize(4)
            .addText('VM Deli\n')
            .addTextSize(2)
            .addText('9556 Rapid City Rd NW\n')
            .addText('Rapid City, MI 49676\n')
            .addText('231-331-6111 ext 3\n')
            .addTextSize(1)
            .addText(`******************************************\n`);
    }
    
    static buttonLockout = false;

    static alerts = {
        newOrderAlert: new notification("/media/audio/newOrder.mp3"),
        timerLow: new notification("/media/audio/timerAlert2.wav", 2),
        notification: new notification("/media/audio/timerAlert.wav", 2)
    }

    static openNotificationWindow(event) {
        if (event) {
            event.preventDefault();
        }
        notificationCard.silenceAll();
        $('#notification_window').removeClass('closed closing opening open').addClass('opening');
        setTimeout(() => {
            $('#notification_window').removeClass('closed closing opening open').addClass('open');
        }, 990);
    }

    static closeNotificationWindow(event) {
        if (event) {
            event.preventDefault();
        }
        $('#notification_window').removeClass('closed closing opening open').addClass('closing');
        setTimeout(() => {
            $('#notification_window').removeClass('closed closing opening open').addClass('closed');
        }, 990);
    }

    startUpdateLoop() {
        this.updateTimer = setInterval(this.update.bind(this), this.UPDATE_INTERVAL_TIME);
        return this;
    }

    stopUpdateLoop() {
        clearInterval(this.updateTimer);
        return this;
    }

    bindButtons() {
        $('.notification-window-opener').on('click', terminal_client.openNotificationWindow);

        $('.order-next-btn, .order-complete-btn').on('click', (event) => {
            event.stopPropagation();
            event.preventDefault();
            if (terminal_client.buttonLockout) return;
            terminal_client.buttonLockout = true;
            var target = $(event.target).parents('.order-next-btn, .order-complete-btn');

            var id = target.data("order-id");
            $.ajax({
                type: "PUT",
                url: "/API/Order/UpdateStatus",
                data: { orderId: id },
                success: this.update.bind(this)
            });
        });

        $('.btn-add-time').on('click', (event) => {
            event.stopPropagation();
            event.preventDefault();
            if (terminal_client.buttonLockout) return;
            terminal_client.buttonLockout = true;
            var target = $(event.target);
            if (!target.hasClass('btn-add-item')) {
                target = $(event.target).parents('.btn-add-time');
            }

            var id = target.data('order-id');
            var time = target.data('time');

            $.ajax({
                type: 'PUT',
                url: '/API/Staff/TerminalService/AddOrderTime',
                data: {
                    orderId: id,
                    time: time
                },
                success: (data, status, xhr) => {
                    if (status == "success") {
                        $('.timer-modal').modal('hide');
                        this.update.call(this);
                    }
                }
            });
        });

        $('.confirm-order-cancel-btn').on('click', (event) => {
            event.stopPropagation();
            event.preventDefault();
            if (terminal_client.buttonLockout) return;
            terminal_client.buttonLockout = true;
            const id = $(event.target).data('order-id');
            $.ajax({
                type: 'DELETE',
                url: '/API/Order/CancelOrder',
                data: {
                    orderNumber: id
                },
                success: (data, status, xhr) => {
                    if (status == "success") {
                        $('.modal').modal('hide');
                        this.update.call(this);
                    }
                }
            });
        });

        $('.order-print-btn').on('click', (event) => {
            event.stopPropagation();
            event.preventDefault();
            const target = retarget(event.target, "order-print-btn");
            if (!this.printer) {
                notificationCard.post("Printer Not Ready.", "An attempt to print was made before the printer was ready to use. Please check the printer status and try again."); 
                return;
            }
            if (terminal_client.buttonLockout) return;
            terminal_client.buttonLockout = true;
            const id = target.data('order-id');


            $.ajax({
                type: 'GET',
                url: `/API/Staff/Orders/Detail/${id}`,
                success: (data, status, xhr) => {
                    if (status == "success") {
                        console.info(data)
                        this.loadPrintHeader();

                        this.printer
                            .addTextSize(3)
                            .addText(`Order #${data.orderNum}\n`)
                            .addTextSize(1)
                            .addTextAlign(this.printer.ALIGN_LEFT)
                            .addText(`\n Name: ${data.customerName}\n`)
                            .addText(`Phone: ${data.customerPhone}\n`)
                            .addTextSize(1)
                            .addText(`******************************************\n`);

                        const barcodes = [];
                        for (var i = 0; i < data.lineItems.length; i++) {
                            const item = `${data.lineItems[i].varient} ${data.lineItems[i].name}`;
                            const price = `$${parseFloat(data.lineItems[i].price).toFixed(2)}`;
                            const fill = 42 - (item.length + price.length);
                            let line = item.padEnd(fill);
                            line += price;

                            this.printer
                                .addText(`${item}\n`);
                            if (data.lineItems[i].additions.length != 0) {
                                this.printer.addText("   Added:\n");
                                for (var j = 0; j < data.lineItems[i].additions.length; j++) {
                                    const aItem = data.lineItems[i].additions[j].name;
                                    const aPrice = `+$${parseFloat(data.lineItems[i].additions[j].price).toFixed(2)}  `;
                                    const aFill = 42 - (5 + aItem.length + aPrice.length);
                                    let aLine = aItem;
                                    for (var k = 0; k < aFill; k++) aLine += ".";
                                    aLine += aPrice;
                                    this.printer.addText(`     ${aLine}\n`);
                                }
                            }

                            if (data.lineItems[i].subtractions.length != 0) {
                                this.printer.addText("   Removed:\n");
                                for (var j = 0; j < data.lineItems[i].subtractions.length; j++) {
                                    const aItem = data.lineItems[i].subtractions[j].name;
                                    this.printer.addText(`     ${aItem}\n`);
                                }
                            }

                            if (data.lineItems[i].comments) {
                                this.printer.addText(`Item Comments: ${data.lineItems[i].comments}\n`);
                            }

                            this.printer
                                .addTextAlign(this.printer.ALIGN_RIGHT)
                                .addTextSize(2)
                                .addText(`${price}\n`)
                                .addTextSize(1)
                                .addTextAlign(this.printer.ALIGN_LEFT)

                            this.printer.addFeedLine(1);
                            const barcodeLabel = item + (data.lineItems[i].modified ? "*" : "");
                            const barcodeData = {
                                upc: data.lineItems[i].upc,
                                label: barcodeLabel
                            };
                            
                            barcodes.push(barcodeData);
                        }
                        const totalsWidth = Math.max(
                            data.subtotal.length,
                            data.tax.length,
                            data.total.length);
                        console.info(totalsWidth)
                        this.printer
                            .addText(`******************************************\n`)
                            .addTextSize(2)
                            .addTextAlign(this.printer.ALIGN_RIGHT)
                            .addText(`Subtotal: ${"".padEnd(totalsWidth - data.subtotal.length, " ")}${data.subtotal}\n`)
                            .addText(`Tax: ${"".padEnd(totalsWidth - data.tax.length, " ")}${data.tax}\n`)
                            .addText(`Total: ${"".padEnd(totalsWidth - data.total.length, " ")}${data.total}\n`)
                            .addTextAlign(this.printer.ALIGN_CENTER)
                            .addTextSize(1)
                            .addText(`******************************************\n`);

                        for (var i = 0; i < barcodes.length; i++) {
                            this.printer
                                .addFeedLine(3)
                                .addText(barcodes[i].label + "\n")
                                .addBarcode(barcodes[i].upc, this.printer.BARCODE_UPC_A, this.printer.HRI_BELOW, this.printer.FONT_A, 4, 120)
                        }

                        this.printer
                            .addFeedLine(3)
                            .addTextAlign(this.printer.ALIGN_RIGHT)
                            .addText("* MODIFIED ITEM\n")
                            .addCut(this.printer.FULL_CUT)
                            .send();

                        terminal_client.buttonLockout = false;
                    }
                }
            });
        });
    }

    async update() {
        terminal_client.buttonLockout = false;
        //check-in with server
        $.ajax({
            type: "PUT",
            url: "/API/Order/TerminalCheckin",
            success: (data, status, xhr) => {
                $.ajax({
                    type: "GET",
                    url: "/API/Staff/TerminalService/Alerts",
                    success: (data, status, xhr) => {
                        if (status == "success") {
                            if (data.newOrderAlert) {
                                terminal_client.alerts.newOrderAlert.play();
                            }
                            if (data.timeLowAlert) {
                                terminal_client.alerts.timerLow.play();
                            }
                        }
                    }
                })
            }

        });


        $.ajax({
            type: "GET",
            url: "/Staff/OrderTerminal/FetchTerminalHeader",
            success: (data, status, xhr) => {
                $('header').html($.parseHTML(data, true));
                let count = 0;
                for (var i = 0; i < notificationCard.active.length; i++) {
                    if (!notificationCard.active[i].silent) count++;
                }
                if (count > 0) {
                    $('.active-notifications').addClass('fa-shake text-warning');
                }
            }
        })

        //check for open modals
        var $infoModalsOpen = $('.info-modal');
        var infoModalId;
        if ($infoModalsOpen && $infoModalsOpen.hasClass('show')) {
            $infoModalsOpen.removeClass('fade');
            $infoModalsOpen.modal('hide');
            infoModalId = $infoModalsOpen.attr('id');
        }

        var $timerModalsOpen = $('.timer-modal');
        var timerModalId;
        if ($timerModalsOpen && $timerModalsOpen.hasClass('show')) {
            $timerModalsOpen.removeClass('fade');
            $timerModalsOpen.modal('hide');
            timerModalId = $timerModalsOpen.attr('id');
        }

        //get updates.
        await $.ajax({
            type: "GET",
            url: "/Staff/OrderTerminal/FetchOrderList/0",
            success: (data, status, xhr) => {
                $('#recieved_orders_body').html(data);
                this.bindButtons();
            }
        })
        await $.ajax({
            type: "GET",
            url: "/Staff/OrderTerminal/FetchOrderList/1",
            success: (data, status, xhr) => {
                $('#in_progress_body').html(data);
                this.bindButtons();
            }
        })
        await $.ajax({
            type: "GET",
            url: "/Staff/OrderTerminal/FetchOrderList/2",
            success: (data, status, xhr) => {
                $('#ready_body').html(data);
                this.bindButtons();
            }
        })
        await $.ajax({
            type: "GET",
            url: "/Staff/OrderTerminal/FetchOrderList/3",
            success: (data, status, xhr) => {
                $('#completed_orders_body').html(data);
                this.bindButtons();
                if (infoModalId) {
                    $(`#${infoModalId}`).removeClass('fade');
                    $(`#${infoModalId}`).modal('show');
                    $(`#${infoModalId}`).addClass('fade');
                }
                if (timerModalId) {
                    $(`#${timerModalId}`).removeClass('fade');
                    $(`#${timerModalId}`).modal('show');
                    $(`#${timerModalId}`).addClass('fade');
                }
            }
        })

    }
}

notificationCard.restoreActiveNotifications();