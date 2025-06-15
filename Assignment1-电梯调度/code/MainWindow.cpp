#include "MainWindow.h"
#include "ElevatorWidget.h"
#include "FloorButtonsWidget.h"
#include "LogPanel.h"
#include "Dispatcher.h" // 引入调度器头文件
#include <QWidget>
#include <QHBoxLayout>

MainWindow::MainWindow(QWidget* parent)
    : QMainWindow(parent),
    elevator1(new Elevator(1, 1, this)),
    elevatorWidget1(new ElevatorWidget(21, this,30)),
    elevator2(new Elevator(2, 1, this)),
    elevatorWidget2(new ElevatorWidget(21, this, 30)),
    elevator3(new Elevator(3, 1, this)),
    elevatorWidget3(new ElevatorWidget(21, this, 30)),
    elevator4(new Elevator(4, 1, this)),
    elevatorWidget4(new ElevatorWidget(21, this, 30)),
    elevator5(new Elevator(5, 1, this)),
    elevatorWidget5(new ElevatorWidget(21, this, 30)),
    buttonsWidget(new FloorButtonsWidget(this)),
    logPanel(new LogPanel(this)),
    dispatcher(new Dispatcher({ elevator1, elevator2, elevator3, elevator4, elevator5}))  // 初始化调度器
{
    setFixedSize(1400, 1000);
    buttonsWidget->setFixedSize(200, 800);

    QWidget* centralWidget = new QWidget(this);
    setCentralWidget(centralWidget);

    QHBoxLayout* layout = new QHBoxLayout(centralWidget);

    layout->addWidget(logPanel, 1);
    layout->addWidget(elevatorWidget1, 2);
    layout->addWidget(elevatorWidget2, 2);
    layout->addWidget(elevatorWidget3, 2);
    layout->addWidget(elevatorWidget4, 2);
    layout->addWidget(elevatorWidget5, 2);
    layout->addWidget(buttonsWidget, 1);

    //绑定外部按钮
    connect(buttonsWidget, &FloorButtonsWidget::floorRequested,
        this, [=](int floor, bool isUp) {
            dispatcher->assign_task(floor, isUp);
        });


    // 电梯1
    connect(elevator1, &Elevator::logMessage, logPanel, &LogPanel::appendLog);
    connect(elevatorWidget1, &ElevatorWidget::floorRequested, elevator1, &Elevator::onFloorRequested);
    connect(elevator1, &Elevator::arrivedAtFloor, elevatorWidget1, &ElevatorWidget::restoreButtonState);
    connect(elevator1, &Elevator::floorChanged, elevatorWidget1, &ElevatorWidget::updateFloorLabel);
    connect(elevator1, &Elevator::openDoorRequested, elevatorWidget1, &ElevatorWidget::openDoor);
    connect(elevator1, &Elevator::closeDoorRequested, elevatorWidget1, &ElevatorWidget::closeDoor);

    // 电梯2
    connect(elevator2, &Elevator::logMessage, logPanel, &LogPanel::appendLog);
    connect(elevatorWidget2, &ElevatorWidget::floorRequested, elevator2, &Elevator::onFloorRequested);
    connect(elevator2, &Elevator::arrivedAtFloor, elevatorWidget2, &ElevatorWidget::restoreButtonState);
    connect(elevator2, &Elevator::floorChanged, elevatorWidget2, &ElevatorWidget::updateFloorLabel);
    connect(elevator2, &Elevator::openDoorRequested, elevatorWidget2, &ElevatorWidget::openDoor);
    connect(elevator2, &Elevator::closeDoorRequested, elevatorWidget2, &ElevatorWidget::closeDoor);
    // 电梯3
    connect(elevator3, &Elevator::logMessage, logPanel, &LogPanel::appendLog);
    connect(elevatorWidget3, &ElevatorWidget::floorRequested, elevator3, &Elevator::onFloorRequested);
    connect(elevator3, &Elevator::arrivedAtFloor, elevatorWidget3, &ElevatorWidget::restoreButtonState);
    connect(elevator3, &Elevator::floorChanged, elevatorWidget3, &ElevatorWidget::updateFloorLabel);
    connect(elevator3, &Elevator::openDoorRequested, elevatorWidget3, &ElevatorWidget::openDoor);
    connect(elevator3, &Elevator::closeDoorRequested, elevatorWidget3, &ElevatorWidget::closeDoor);
    // 电梯4
    connect(elevator4, &Elevator::logMessage, logPanel, &LogPanel::appendLog);
    connect(elevatorWidget4, &ElevatorWidget::floorRequested, elevator4, &Elevator::onFloorRequested);
    connect(elevator4, &Elevator::arrivedAtFloor, elevatorWidget4, &ElevatorWidget::restoreButtonState);
    connect(elevator4, &Elevator::floorChanged, elevatorWidget4, &ElevatorWidget::updateFloorLabel);
    connect(elevator4, &Elevator::openDoorRequested, elevatorWidget4, &ElevatorWidget::openDoor);
    connect(elevator4, &Elevator::closeDoorRequested, elevatorWidget4, &ElevatorWidget::closeDoor);
    // 电梯5
    connect(elevator5, &Elevator::logMessage, logPanel, &LogPanel::appendLog);
    connect(elevatorWidget5, &ElevatorWidget::floorRequested, elevator5, &Elevator::onFloorRequested);
    connect(elevator5, &Elevator::arrivedAtFloor, elevatorWidget5, &ElevatorWidget::restoreButtonState);
    connect(elevator5, &Elevator::floorChanged, elevatorWidget5, &ElevatorWidget::updateFloorLabel);
    connect(elevator5, &Elevator::openDoorRequested, elevatorWidget5, &ElevatorWidget::openDoor);
    connect(elevator5, &Elevator::closeDoorRequested, elevatorWidget5, &ElevatorWidget::closeDoor);
}

MainWindow::~MainWindow() {
    delete buttonsWidget;
    delete logPanel;
    delete elevator1;
    delete elevatorWidget1;
    delete elevator2;
    delete elevatorWidget2;
    delete elevator3;
    delete elevatorWidget3;
    delete elevator4;
    delete elevatorWidget4;
    delete elevator5;
    delete elevatorWidget5;
    delete dispatcher;  
}
