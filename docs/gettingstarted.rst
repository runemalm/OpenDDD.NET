############
Installation
############

Install the framework using the package manager or the `dotnet CLI <https://learn.microsoft.com/en-us/nuget/consume-packages/install-use-packages-dotnet-cli>`_::

    $ dotnet add package OpenDDD.NET


###################
Example Application
###################

The open source project ``PowerIAM`` is built with this framework. Check out the `source code <https://...>`_ on the project's github repository for an in-depth full example of how to implement your bounded contexts using OpenDDD.NET.

Another easy way to get started quickly is to use the `WeatherForecast <https://todo>`_ project templates.

You can always go back to the :doc:`start page<index>` of this documentation for some snippets of code.

Next, we'll continue this guide by going through the basic concepts.


#########
Env files
#########

An `Env file <https://12factor.net/config>`_ is where you put the configuration for a specific environment.

You will e.g. have the following env files, one each for all of your environments:

- env.local.pycharm
- env.local.minikube
- env.local.test
- env.pipeline.test
- env.staging
- env (production)

Env files are part of `The Twelve-Factor App <https://12factor.net>`_ pattern.


###############
Building Blocks
###############

These are the ``building blocks`` used to implement the business rules of the domain model:

* :ref:`Entity`
* :ref:`Action`
* :ref:`Event`
* :ref:`Listener`

The :ref:`Config` object will be accessible from anywhere in your code. This object will contain all the configuration settings of your bounded context. Use the :ref:`Env files` to define these settings for each of the environments your context will run in. These environments will typically be local, dev, staging and production, or any subset of them. By utilizing :ref:`Env files`, you can avoid the "configuration hell" where settings are scattered across different locations, and/or hard-coded into the application code, making it impossible to configure the context for different environments without a new code release.

The config object will be registered with the :ref:`Dependency manager`. This is how you will be able to access it from all over the code. The config objects is not the only dependency you will register with the :ref:`Dependency manager`. You can register any depdency that you want to be able to reference from anywhere in the code. The dependency manager brings the inversion of control principle and the dependency injection pattern to the framework.

The creation of dependencies are done in the :ref:`main.py` file. Here they are also registered with the Dependency Manager. The final step in main.py is to instantiate a :ref:`Container` object and schedule it to run on the python event loop.

The :ref:`Makefile` is used to automate your daily tasks so that you can run them easily and quickly from the command line. These tasks are e.g. building, running the unit tests, running your deployment pipeline locally, etc. You can add your own makefile targets to this file as you wish, but there are some standard tasks that follows with the project template setup. Check out the `shipping makefile example <https://github.com/runemalm/ddd-for-python/tree/master/examples/webshop/shipping/Makefile?at=master>`_ for inspiration.


Entity
------

The :class:`~ddd.application.config.Config` object holds all the configuration settings of your bounded context .

If you don't have custom settings added to your env file, you can simply instantiate a config object from the base :class:`~ddd.application.config.Config` class. If you do have custom settings however, which you typically do, you need to subclass the base :class:`~ddd.application.config.Config` and override a couple of methods to instruct :class:`~ddd.application.config.Config` where to find the new settings in the env file and where in the :class:`~ddd.application.config.Config` object to store them. 

This is an example of how you subclass the base Config class for the shipping context:

Start by creating ``<your_domain>.application.config``::

    touch <your_product>/<your_domain>/application/config.py

Then open ``config.py`` in your favourite text editor::

    subl <your_product>/<your_domain>/application/config.py


Add the class declaration::

    from ddd.application.config import Config as BaseConfig


    class Config(BaseConfig):
        def __init__(self):
            super().__init__()

Then override :meth:`~ddd.application.config.Config._declare_settings` to declare the new settings::

    def _declare_settings(self):
        self.my_custom_setting = None
        super()._declare_settings()

Override :meth:`~ddd.application.config.Config._read_config` to define which environment variables that contains the new settings::

    def _read_config(self):
        self.my_custom_setting = os.getenv('MY_CUSTOM_SETTING')

.. note:: :class:`~ddd.application.config.Config` knows how to find the env file and read it's settings as long as it's placed in the root of the project and named "env". If you want another name for your env file, you must pass the path using the ``env_file_path`` argument of the constructor.  

Now you can reference your custom settings from the config object like so::

    config = dep_mgr.get_config()

    my_custom_setting = config.my_custom_setting

    print("My custom setting:", my_custom_setting)


Action
------

Describe this..


Event
-----

As previously mentioned, the ``main.py`` file can be seen as the starting point for your code that will execute in the container and implement the bounded context.

That means this file will instantiate all the building blocks that comprises the context and then schedule it to run on the event loop.

This is how the `shipping example main.py <https://github.com/runemalm/ddd-for-python/tree/master/examples/webshop/shipping/src/main.py?at=master>`_ file looks like::

    from ddd.application.config import Config
    from ddd.infrastructure.container import Container

    from shipping.utils.dep_mgr import DependencyManager
    from shipping.application.shipping_application_service import \
    ShippingApplicationService


    if __name__ == "__main__":
        """
        This is the container entry point.   
        Creates the app and runs it in the container.
        """

        # Config
        config = Config()

        # Dependency manager
        dep_mgr = \
            DependencyManager(
                config=config,
            )

        # Application service
        service = \
            ShippingApplicationService(
                customer_repository=dep_mgr.get_customer_repository(),
                db_service=dep_mgr.get_db_service(),
                domain_adapter=dep_mgr.get_domain_adapter(),
                domain_publisher=dep_mgr.get_domain_publisher(),
                event_repository=dep_mgr.get_event_repository(),
                interchange_adapter=dep_mgr.get_interchange_adapter(),
                interchange_publisher=dep_mgr.get_interchange_publisher(),
                job_adapter=dep_mgr.get_job_adapter(),
                job_service=dep_mgr.get_job_service(),
                log_service=dep_mgr.get_log_service(),
                scheduler_adapter=dep_mgr.get_scheduler_adapter(),
                shipment_repository=dep_mgr.get_shipment_repository(),
                max_concurrent_actions=config.max_concurrent_actions,
                loop=config.loop.instance,
            )

        # ..register
        dep_mgr.set_service(service)

        # Container
        container = \
            Container(
                app_service=service,
                log_service=dep_mgr.get_log_service(),
            )

        # ..run
        loop = config.loop.instance
        loop.run_until_complete(container.run())
        loop.close()


.. note:: The Container will listen to UNIX stop signals (e.g. by a user pressing Ctrl+C in the terminal, or by the Docker Engine stopping the container, for any reason). Upon receiving such a stop signal, it gracefully shuts down the context by first calling :meth:`~ddd.application.application_service.ApplicationService.stop` on the ApplicationService, which in turns calls :meth:`~ddd.infrastructure.adapters.Adapter.stop` on all the secondary- and primary adapters (in that order). The Container task is then taken off the event loop and the docker container can be destroyed by the orchestrator.


Listener
--------

Describe this...


#############
Serialization
#############

Describe this...


#####
Tests
#####

Describe this...


###############
Troubleshooting
###############

If you suspect something in the ddd package isn't as expected, it will be helpful to increase the logging level of the
framework to the ``DEBUG`` level in the ``env file`` like this::

    CFG_LOGGING_LEVEL=Debug

This should provide lots of useful information about what's going on inside the openddd.net core.
